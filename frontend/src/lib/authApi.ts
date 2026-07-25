import { api } from './api'
import type {
  AuthResponse,
  ConfirmPasswordResetRequest,
  LoginRequest,
  RegisterRequest,
  RequestPasswordResetRequest,
} from '../types/auth'

export const authApi = {
  register: (data: RegisterRequest) => api.post<AuthResponse>('/auth/register', data),
  login: (data: LoginRequest) => api.post<AuthResponse>('/auth/login', data),
  requestPasswordReset: (data: RequestPasswordResetRequest) =>
    api.post<void>('/auth/forgot-password', data),
  confirmPasswordReset: (data: ConfirmPasswordResetRequest) =>
    api.post<void>('/auth/reset-password', data),
}
