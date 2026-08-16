import { postApiData, storeAccessToken } from './client'
import type { AuthResponse } from '../types/api'

/** POST /api/auth/login → { accessToken, expiresAtUtc, user } inside the success envelope. */
export async function login(email: string, password: string): Promise<AuthResponse> {
  const result = await postApiData<AuthResponse>('/api/auth/login', { email, password })
  storeAccessToken(result.accessToken)
  return result
}
