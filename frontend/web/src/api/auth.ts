import type { AuthResponse, CurrentUser } from '../types/api'
import { getApiData, sendApiData, setAccessToken, setCurrentUser } from './client'

export async function login(email: string, password: string) {
  const payload = await sendApiData<AuthResponse>('post', '/api/auth/login', { email, password })
  setAccessToken(payload.accessToken)
  setCurrentUser(payload.user)
  return payload
}

export function logout() {
  setAccessToken(null)
}

export async function fetchCurrentUser() {
  const user = await getApiData<CurrentUser>('/api/auth/me')
  setCurrentUser(user)
  return user
}
