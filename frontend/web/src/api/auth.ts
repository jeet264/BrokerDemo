import { http, setAccessToken } from './client'
import type { ApiResponse, AuthResponse } from '../types/api'

export async function login(email: string, password: string) {
  const response = await http.post<ApiResponse<AuthResponse>>('/api/auth/login', { email, password })
  const payload = response.data
  if (!payload.success || payload.data == null) {
    throw new Error(payload.message ?? 'Sign-in failed.')
  }

  setAccessToken(payload.data.accessToken)
  return payload.data
}

export function logout() {
  setAccessToken(null)
}
