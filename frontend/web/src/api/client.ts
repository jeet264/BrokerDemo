import axios from 'axios'
import type { FieldValues, Path, UseFormSetError } from 'react-hook-form'
import type { ApiResponse, CurrentUser } from '../types/api'

function apiBaseUrl(): string {
  const configured = import.meta.env.VITE_API_BASE_URL
  // `same-origin` (or empty) uses the Vite / Caddy proxy so a public tunnel only needs one URL.
  if (!configured || configured === 'same-origin') {
    return ''
  }

  return configured
}

const baseURL = apiBaseUrl()
const tokenKey = 'brokeros.accessToken'
const userKey = 'brokeros.currentUser'

export class ApiRequestError extends Error {
  readonly errors: { field?: string; message: string }[]

  constructor(message: string, errors: { field?: string; message: string }[] = []) {
    super(message)
    this.name = 'ApiRequestError'
    this.errors = errors
  }
}

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

  // Let the browser set multipart boundaries — a hardcoded JSON content-type breaks file uploads.
  if (typeof FormData !== 'undefined' && config.data instanceof FormData) {
    delete config.headers['Content-Type']
  }

  return config
})

export function setAccessToken(token: string | null) {
  if (token) {
    localStorage.setItem(tokenKey, token)
  } else {
    localStorage.removeItem(tokenKey)
    setCurrentUser(null)
  }
}

export function getAccessToken() {
  return localStorage.getItem(tokenKey)
}

export function setCurrentUser(user: CurrentUser | null) {
  if (user) {
    localStorage.setItem(userKey, JSON.stringify(user))
  } else {
    localStorage.removeItem(userKey)
  }
}

export function getCurrentUser(): CurrentUser | null {
  const raw = localStorage.getItem(userKey)
  if (!raw) {
    return null
  }

  try {
    return JSON.parse(raw) as CurrentUser
  } catch {
    localStorage.removeItem(userKey)
    return null
  }
}

export async function getApiData<T>(url: string): Promise<T> {
  try {
    const response = await http.get<ApiResponse<T>>(url)
    return unwrap(response.data)
  } catch (error) {
    rethrow(error)
  }
}

export async function sendApiData<T>(
  method: 'post' | 'put' | 'delete',
  url: string,
  body?: unknown,
  timeout?: number,
): Promise<T> {
  try {
    const response = await http.request<ApiResponse<T>>({ method, url, data: body, timeout })
    return unwrap(response.data)
  } catch (error) {
    rethrow(error)
  }
}

export function postApiData<T>(url: string, body: unknown): Promise<T> {
  return sendApiData<T>('post', url, body)
}

export function postFormData<T>(url: string, form: FormData): Promise<T> {
  return sendApiData<T>('post', url, form, 120000)
}

export function applyApiFieldErrors<T extends FieldValues>(
  error: unknown,
  setError: UseFormSetError<T>,
) {
  if (!(error instanceof ApiRequestError)) {
    return
  }

  for (const item of error.errors) {
    if (!item.field) {
      continue
    }

    setError(item.field as Path<T>, { type: 'server', message: item.message })
  }
}

function unwrap<T>(payload: ApiResponse<T>): T {
  if (!payload.success || payload.data == null) {
    throw new ApiRequestError(payload.message ?? 'The request could not be completed.', payload.errors ?? [])
  }

  return payload.data
}

function rethrow(error: unknown): never {
  if (error instanceof ApiRequestError) {
    throw error
  }

  if (axios.isAxiosError(error)) {
    const payload = error.response?.data as ApiResponse<unknown> | undefined
    if (payload && typeof payload === 'object') {
      throw new ApiRequestError(payload.message ?? 'The request could not be completed.', payload.errors ?? [])
    }
  }

  throw error
}
