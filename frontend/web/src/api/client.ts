import axios from 'axios'
import type { FieldValues, Path, UseFormSetError } from 'react-hook-form'
import type { ApiResponse } from '../types/api'

const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'
const tokenKey = 'brokeros.accessToken'

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
  try {
    const response = await http.get<ApiResponse<T>>(url)
    return unwrap(response.data)
  } catch (error) {
    rethrow(error)
  }
}

export async function sendApiData<T>(
  method: 'post' | 'put',
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
