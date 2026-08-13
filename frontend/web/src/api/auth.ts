import type { ApiResponse, AuthResponse, CurrentUser } from '../types/api'
import { getApiData, http, setAccessToken, setCurrentUser } from './client'

export async function login(email: string, password: string) {
  const response = await http.post<ApiResponse<AuthResponse>>('/api/auth/login', { email, password })
  const payload = response.data
  if (!payload.success || payload.data == null) {
    throw new Error(payload.message ?? 'Sign-in failed.')
  }

  setAccessToken(payload.data.accessToken)
  setCurrentUser(payload.data.user)
  return payload.data
}

export function logout() {
  setAccessToken(null)
}

export async function fetchCurrentUser() {
  const user = await getApiData<CurrentUser>('/api/auth/me')
  setCurrentUser(user)
  return user
}
