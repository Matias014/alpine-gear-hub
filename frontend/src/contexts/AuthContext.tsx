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
  const [user, setUser] = useState<AuthUser | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const stored = tokenStorage.get()
    if (stored) setUser(toUser(stored))
    setIsLoading(false)

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
