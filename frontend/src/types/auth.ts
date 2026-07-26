export type UserRole = 'Member' | 'Moderator' | 'Admin'

// No refreshToken field - it now travels only as an httpOnly cookie the backend sets on
// register/login/refresh (see lib/api.ts), never in a body a script could read.
export interface AuthResponse {
  accessToken: string
  accessTokenExpiresAt: string
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

export interface RequestPasswordResetRequest {
  email: string
}

export interface ConfirmPasswordResetRequest {
  token: string
  newPassword: string
}

export interface ConfirmEmailRequest {
  token: string
}

export interface ResendEmailConfirmationRequest {
  email: string
}
