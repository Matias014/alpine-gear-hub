import { api } from './api'
import type {
  AuthResponse,
  ConfirmEmailRequest,
  ConfirmPasswordResetRequest,
  LoginRequest,
  RegisterRequest,
  RequestPasswordResetRequest,
  ResendEmailConfirmationRequest,
} from '../types/auth'

export const authApi = {
  register: (data: RegisterRequest) => api.post<AuthResponse>('/auth/register', data),
  login: (data: LoginRequest) => api.post<AuthResponse>('/auth/login', data),
  requestPasswordReset: (data: RequestPasswordResetRequest) =>
    api.post<void>('/auth/forgot-password', data),
  confirmPasswordReset: (data: ConfirmPasswordResetRequest) =>
    api.post<void>('/auth/reset-password', data),
  confirmEmail: (data: ConfirmEmailRequest) => api.post<void>('/auth/confirm-email', data),
  resendEmailConfirmation: (data: ResendEmailConfirmationRequest) =>
    api.post<void>('/auth/resend-confirmation', data),
}
