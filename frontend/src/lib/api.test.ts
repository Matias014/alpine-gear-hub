import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { api } from './api'
import { setOnAuthExpired, tokenStorage } from './tokenStorage'
import type { AuthResponse } from '../types/auth'

function fakeResponse(status: number, body: unknown) {
  return {
    status,
    ok: status >= 200 && status < 300,
    statusText: status === 401 ? 'Unauthorized' : 'Error',
    json: async () => body,
  } as Response
}

const storedAuth: AuthResponse = {
  accessToken: 'old-token',
  accessTokenExpiresAt: '2026-01-01T00:00:00Z',
  refreshToken: 'refresh-token',
  fullName: 'Jane Climber',
  email: 'jane@example.com',
  role: 'Member',
}

beforeEach(() => {
  localStorage.clear()
  setOnAuthExpired(() => {})
})

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('api request headers', () => {
  it('attaches a bearer token when one is stored', async () => {
    tokenStorage.set(storedAuth)
    const fetchMock = vi.fn(async (_url: string, _init?: RequestInit) => fakeResponse(200, { ok: true }))
    vi.stubGlobal('fetch', fetchMock)

    await api.get('/listings')

    const [url, init] = fetchMock.mock.calls[0]
    expect(url).toBe('/api/listings')
    expect((init?.headers as Record<string, string>).Authorization).toBe('Bearer old-token')
  })

  it('omits the Authorization header when nothing is stored', async () => {
    const fetchMock = vi.fn(async (_url: string, _init?: RequestInit) => fakeResponse(200, { ok: true }))
    vi.stubGlobal('fetch', fetchMock)

    await api.get('/listings')

    const [, init] = fetchMock.mock.calls[0]
    expect((init?.headers as Record<string, string>).Authorization).toBeUndefined()
  })

  it('sends JSON content-type for post/put/delete but not for uploads', async () => {
    const fetchMock = vi.fn(async (_url: string, _init?: RequestInit) => fakeResponse(200, { ok: true }))
    vi.stubGlobal('fetch', fetchMock)

    await api.post('/listings', { title: 'Rope' })
    const [, postInit] = fetchMock.mock.calls[0]
    expect((postInit?.headers as Record<string, string>)['Content-Type']).toBe('application/json')

    await api.upload('/listings/1/images', new File(['x'], 'photo.png'))
    const [, uploadInit] = fetchMock.mock.calls[1]
    expect((uploadInit?.headers as Record<string, string>)['Content-Type']).toBeUndefined()
  })
})

describe('api response handling', () => {
  it('returns undefined for a 204 response', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => fakeResponse(204, null)))
    await expect(api.delete('/listings/1')).resolves.toBeUndefined()
  })

  it('throws using ProblemDetails.detail when present', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn(async () => fakeResponse(400, { title: 'Bad Request', detail: 'Price must be greater than zero' })),
    )
    await expect(api.get('/listings')).rejects.toThrow('Price must be greater than zero')
  })

  it('falls back to title, then to a generic message, when detail is missing', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => fakeResponse(500, { title: 'Server Error' })))
    await expect(api.get('/listings')).rejects.toThrow('Server Error')

    vi.stubGlobal('fetch', vi.fn(async () => fakeResponse(500, {})))
    await expect(api.get('/listings')).rejects.toThrow('Request failed')
  })
})

describe('silent token refresh on 401', () => {
  it('refreshes the access token and retries the original request once', async () => {
    tokenStorage.set(storedAuth)
    const fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
      const authHeader = (init?.headers as Record<string, string> | undefined)?.Authorization
      if (url === '/api/auth/refresh') {
        return fakeResponse(200, { ...storedAuth, accessToken: 'new-token', refreshToken: 'new-refresh' })
      }
      if (authHeader === 'Bearer new-token') return fakeResponse(200, { items: [] })
      return fakeResponse(401, { title: 'Unauthorized' })
    })
    vi.stubGlobal('fetch', fetchMock)

    const result = await api.get('/listings')

    expect(result).toEqual({ items: [] })
    expect(tokenStorage.get()?.accessToken).toBe('new-token')
    const calledPaths = fetchMock.mock.calls.map((call) => call[0])
    expect(calledPaths).toEqual(['/api/listings', '/api/auth/refresh', '/api/listings'])
  })

  it('dedupes concurrent refreshes into a single refresh call', async () => {
    tokenStorage.set(storedAuth)
    const fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
      const authHeader = (init?.headers as Record<string, string> | undefined)?.Authorization
      if (url === '/api/auth/refresh') {
        return fakeResponse(200, { ...storedAuth, accessToken: 'new-token', refreshToken: 'new-refresh' })
      }
      if (authHeader === 'Bearer new-token') return fakeResponse(200, { ok: true })
      return fakeResponse(401, { title: 'Unauthorized' })
    })
    vi.stubGlobal('fetch', fetchMock)

    await Promise.all([api.get('/listings'), api.get('/messages')])

    const refreshCalls = fetchMock.mock.calls.filter((call) => call[0] === '/api/auth/refresh')
    expect(refreshCalls).toHaveLength(1)
  })

  it('clears storage and notifies auth-expired listeners when the refresh itself fails', async () => {
    tokenStorage.set(storedAuth)
    const authExpired = vi.fn()
    setOnAuthExpired(authExpired)

    const fetchMock = vi.fn(async (url: string) => {
      if (url === '/api/auth/refresh') return fakeResponse(401, { title: 'invalid refresh token' })
      return fakeResponse(401, { detail: 'Session expired' })
    })
    vi.stubGlobal('fetch', fetchMock)

    await expect(api.get('/listings')).rejects.toThrow('Session expired')
    expect(tokenStorage.get()).toBeNull()
    expect(authExpired).toHaveBeenCalledOnce()
  })

  it('does not attempt a refresh for auth endpoints themselves', async () => {
    const fetchMock = vi.fn(async () => fakeResponse(401, { detail: 'Invalid credentials' }))
    vi.stubGlobal('fetch', fetchMock)

    await expect(api.post('/auth/login', { email: 'a@b.com', password: 'x' })).rejects.toThrow('Invalid credentials')
    expect(fetchMock).toHaveBeenCalledOnce()
  })
})
