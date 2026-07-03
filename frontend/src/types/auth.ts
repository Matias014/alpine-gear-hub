export type UserRole = 'Member' | 'Moderator' | 'Admin'

export interface AuthResponse {
  accessToken: string
  accessTokenExpiresAt: string
  refreshToken: string
  fullName: string
  email: string
  role: UserRole
}

export interface RegisterRequest {
  fullName: string
  email: string
  password: string
}

export interface LoginRequest {
  email: string
  password: string
}
