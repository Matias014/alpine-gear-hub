import type { AuthResponse } from '../types/auth'
import { notifyAuthExpired, tokenStorage } from './tokenStorage'

const BASE_URL = '/api'
const AUTH_PATHS_WITHOUT_RETRY = ['/auth/login', '/auth/register', '/auth/refresh']

// Dedupes concurrent refresh attempts - without this, several requests hitting a stale token
// at once would each kick off their own refresh and race each other on the (rotating) token.
let refreshPromise: Promise<string | null> | null = null

async function refreshAccessToken(): Promise<string | null> {
  const stored = tokenStorage.get()
  if (!stored) return null

  refreshPromise ??= (async () => {
    try {
      const res = await fetch(`${BASE_URL}/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken: stored.refreshToken }),
      })
      if (!res.ok) throw new Error('refresh failed')

      const auth = (await res.json()) as AuthResponse
      tokenStorage.set(auth)
      return auth.accessToken
    } catch {
      tokenStorage.clear()
      notifyAuthExpired()
      return null
    } finally {
      refreshPromise = null
    }
  })()

  return refreshPromise
}

async function request<T>(path: string, init?: RequestInit, isRetry = false): Promise<T> {
  const token = tokenStorage.get()?.accessToken

  const res = await fetch(`${BASE_URL}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...init?.headers,
    },
  })

  if (res.status === 401 && !isRetry && !AUTH_PATHS_WITHOUT_RETRY.includes(path)) {
    const newToken = await refreshAccessToken()
    if (newToken) return request<T>(path, init, true)
  }

  if (!res.ok) {
    // ProblemDetails puts the useful message in `detail` - `title` is just the HTTP status category.
    const error = await res.json().catch(() => ({ title: res.statusText }))
    throw new Error(error.detail ?? error.title ?? 'Request failed')
  }

  if (res.status === 204) return undefined as T
  return res.json()
}

export const api = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: 'POST', body: body !== undefined ? JSON.stringify(body) : undefined }),
  put: <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'PUT', body: JSON.stringify(body) }),
  delete: <T>(path: string) => request<T>(path, { method: 'DELETE' }),
}
