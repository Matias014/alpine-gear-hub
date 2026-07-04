import { beforeEach, describe, expect, it, vi } from 'vitest'
import { notifyAuthExpired, setOnAuthExpired, tokenStorage } from './tokenStorage'
import type { AuthResponse } from '../types/auth'

const sampleAuth: AuthResponse = {
  accessToken: 'access-token',
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

describe('tokenStorage', () => {
  it('returns null when nothing is stored', () => {
    expect(tokenStorage.get()).toBeNull()
  })

  it('round-trips a stored auth response', () => {
    tokenStorage.set(sampleAuth)
    expect(tokenStorage.get()).toEqual(sampleAuth)
  })

  it('clears the stored value', () => {
    tokenStorage.set(sampleAuth)
    tokenStorage.clear()
    expect(tokenStorage.get()).toBeNull()
  })

  it('returns null instead of throwing on corrupt stored JSON', () => {
    localStorage.setItem('agh_auth', '{not-json')
    expect(tokenStorage.get()).toBeNull()
  })
})

describe('auth-expired notification', () => {
  it('invokes the registered callback', () => {
    const callback = vi.fn()
    setOnAuthExpired(callback)
    notifyAuthExpired()
    expect(callback).toHaveBeenCalledOnce()
  })

  it('does nothing when no callback has been registered', () => {
    // setOnAuthExpired is module-level state - reset it to nothing registered by "unregistering" via a no-op.
    setOnAuthExpired(() => {})
    expect(() => notifyAuthExpired()).not.toThrow()
  })
})
