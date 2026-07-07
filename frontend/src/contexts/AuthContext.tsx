import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { authApi } from '../lib/authApi'
import { decodeJwtSubject } from '../lib/jwt'
import { setOnAuthExpired, tokenStorage } from '../lib/tokenStorage'
import type { AuthResponse, LoginRequest, RegisterRequest, UserRole } from '../types/auth'

interface AuthUser {
  id: string
  fullName: string
  email: string
  role: UserRole
}

interface AuthContextValue {
  user: AuthUser | null
  isAuthenticated: boolean
  isLoading: boolean
  login: (data: LoginRequest) => Promise<void>
  register: (data: RegisterRequest) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

function toUser(auth: AuthResponse): AuthUser | null {
  const id = decodeJwtSubject(auth.accessToken)
  if (!id) return null
  return { id, fullName: auth.fullName, email: auth.email, role: auth.role }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  // Lazy initializer instead of hydrating in an effect - tokenStorage.get() is a pure
  // synchronous read, so there's no need for an extra render pass (and an isLoading flag)
  // just to catch up to a value we could compute on the first render directly.
  const [user, setUser] = useState<AuthUser | null>(() => {
    const stored = tokenStorage.get()
    return stored ? toUser(stored) : null
  })
  const isLoading = false

  useEffect(() => {
    setOnAuthExpired(() => setUser(null))
  }, [])

  async function login(data: LoginRequest) {
    const auth = await authApi.login(data)
    tokenStorage.set(auth)
    setUser(toUser(auth))
  }

  async function register(data: RegisterRequest) {
    const auth = await authApi.register(data)
    tokenStorage.set(auth)
    setUser(toUser(auth))
  }

  function logout() {
    tokenStorage.clear()
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, isAuthenticated: user !== null, isLoading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within an AuthProvider')
  return ctx
}
