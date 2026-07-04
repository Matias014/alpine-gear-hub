import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { AuthProvider, useAuth } from './AuthContext'
import { authApi } from '../lib/authApi'
import { tokenStorage } from '../lib/tokenStorage'
import type { AuthResponse } from '../types/auth'

vi.mock('../lib/authApi', () => ({
  authApi: { login: vi.fn(), register: vi.fn() },
}))

function makeToken(sub: string): string {
  const base64url = (obj: object) =>
    btoa(JSON.stringify(obj)).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '')
  return `${base64url({ alg: 'HS256' })}.${base64url({ sub })}.signature`
}

function makeAuthResponse(overrides: Partial<AuthResponse> = {}): AuthResponse {
  return {
    accessToken: makeToken('user-1'),
    accessTokenExpiresAt: '2026-01-01T00:00:00Z',
    refreshToken: 'refresh-token',
    fullName: 'Jane Climber',
    email: 'jane@example.com',
    role: 'Member',
    ...overrides,
  }
}

function Consumer() {
  const { user, isAuthenticated, isLoading, login, register, logout } = useAuth()
  return (
    <div>
      <p>loading: {String(isLoading)}</p>
      <p>authenticated: {String(isAuthenticated)}</p>
      <p>user: {user ? `${user.fullName} (${user.role})` : 'none'}</p>
      <button onClick={() => login({ email: 'jane@example.com', password: 'x' })}>Log in</button>
      <button onClick={() => register({ fullName: 'Jane Climber', email: 'jane@example.com', password: 'x' })}>
        Register
      </button>
      <button onClick={logout}>Log out</button>
    </div>
  )
}

beforeEach(() => {
  localStorage.clear()
  vi.clearAllMocks()
})

describe('AuthProvider', () => {
  it('starts unauthenticated when nothing is stored', async () => {
    render(
      <AuthProvider>
        <Consumer />
      </AuthProvider>,
    )

    await waitFor(() => expect(screen.getByText('loading: false')).toBeInTheDocument())
    expect(screen.getByText('authenticated: false')).toBeInTheDocument()
    expect(screen.getByText('user: none')).toBeInTheDocument()
  })

  it('hydrates the user from a previously stored auth response', async () => {
    tokenStorage.set(makeAuthResponse({ fullName: 'Stored User' }))

    render(
      <AuthProvider>
        <Consumer />
      </AuthProvider>,
    )

    await waitFor(() => expect(screen.getByText('user: Stored User (Member)')).toBeInTheDocument())
    expect(screen.getByText('authenticated: true')).toBeInTheDocument()
  })

  it('logs in, persists the session, and exposes the resulting user', async () => {
    vi.mocked(authApi.login).mockResolvedValue(makeAuthResponse({ fullName: 'Fresh Login' }))
    const user = userEvent.setup()

    render(
      <AuthProvider>
        <Consumer />
      </AuthProvider>,
    )
    await waitFor(() => expect(screen.getByText('loading: false')).toBeInTheDocument())

    await user.click(screen.getByText('Log in'))

    await waitFor(() => expect(screen.getByText('user: Fresh Login (Member)')).toBeInTheDocument())
    expect(tokenStorage.get()?.fullName).toBe('Fresh Login')
  })

  it('registers and exposes the resulting user', async () => {
    vi.mocked(authApi.register).mockResolvedValue(makeAuthResponse({ fullName: 'New Signup' }))
    const user = userEvent.setup()

    render(
      <AuthProvider>
        <Consumer />
      </AuthProvider>,
    )
    await waitFor(() => expect(screen.getByText('loading: false')).toBeInTheDocument())

    await user.click(screen.getByText('Register'))

    await waitFor(() => expect(screen.getByText('user: New Signup (Member)')).toBeInTheDocument())
  })

  it('logs out, clearing both state and storage', async () => {
    tokenStorage.set(makeAuthResponse())
    const user = userEvent.setup()

    render(
      <AuthProvider>
        <Consumer />
      </AuthProvider>,
    )
    await waitFor(() => expect(screen.getByText('authenticated: true')).toBeInTheDocument())

    await user.click(screen.getByText('Log out'))

    await waitFor(() => expect(screen.getByText('authenticated: false')).toBeInTheDocument())
    expect(tokenStorage.get()).toBeNull()
  })
})

describe('useAuth', () => {
  it('throws when used outside an AuthProvider', () => {
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => {})
    expect(() => render(<Consumer />)).toThrow('useAuth must be used within an AuthProvider')
    consoleError.mockRestore()
  })
})
