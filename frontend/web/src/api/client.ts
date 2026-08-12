import axios from 'axios'
import type { ApiResponse } from '../types/api'

const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'
const tokenKey = 'brokeros.accessToken'

export const http = axios.create({
  baseURL,
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json',
  },
})

http.interceptors.request.use((config) => {
  const token = localStorage.getItem(tokenKey)
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

export function setAccessToken(token: string | null) {
  if (token) {
    localStorage.setItem(tokenKey, token)
  } else {
    localStorage.removeItem(tokenKey)
  }
}

export function getAccessToken() {
  return localStorage.getItem(tokenKey)
}

export async function getApiData<T>(url: string): Promise<T> {
  const response = await http.get<ApiResponse<T>>(url)
  return unwrap(response.data)
}

export async function sendApiData<T>(method: 'post' | 'put', url: string, body?: unknown): Promise<T> {
  const response = await http.request<ApiResponse<T>>({ method, url, data: body })
  return unwrap(response.data)
}

function unwrap<T>(payload: ApiResponse<T>): T {
  if (!payload.success || payload.data == null) {
    throw new Error(payload.message ?? 'The request could not be completed.')
  }

  return payload.data
}
