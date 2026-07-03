import { api } from './api'
import type { AuthResponse, LoginRequest, RegisterRequest } from '../types/auth'

export const authApi = {
  register: (data: RegisterRequest) => api.post<AuthResponse>('/auth/register', data),
  login: (data: LoginRequest) => api.post<AuthResponse>('/auth/login', data),
}
