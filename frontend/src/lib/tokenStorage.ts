import type { AuthResponse } from '../types/auth'

const STORAGE_KEY = 'agh_auth'

let onAuthExpired: (() => void) | null = null

export const tokenStorage = {
  get(): AuthResponse | null {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return null
    try {
      return JSON.parse(raw) as AuthResponse
    } catch {
      return null
    }
  },
  set(auth: AuthResponse) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(auth))
  },
  clear() {
    localStorage.removeItem(STORAGE_KEY)
  },
}

// api.ts calls this when a silent refresh fails, so AuthContext (registered on mount) can
// clear its React state too - localStorage changes alone won't trigger a re-render.
export function setOnAuthExpired(callback: () => void) {
  onAuthExpired = callback
}

export function notifyAuthExpired() {
  onAuthExpired?.()
}
