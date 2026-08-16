import axios, { AxiosError } from 'axios'
import type { ApiResponse } from '../types/api'

/**
 * Axios instance for the BrokerOS API.
 * Base URL: VITE_API_BASE_URL, or http://localhost:5000 in local dev.
 * JSON bodies use the envelope { success, data, message, errors, traceId }.
 */
const TOKEN_KEY = 'brokeros.accessToken'
const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'

export const http = axios.create({
  baseURL,
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json',
  },
})

http.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_KEY)
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  // Let the browser set multipart boundaries — a hardcoded JSON content-type breaks file uploads.
  if (typeof FormData !== 'undefined' && config.data instanceof FormData) {
    delete config.headers['Content-Type']
  }

  return config
})

export function getStoredAccessToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function storeAccessToken(token: string) {
  localStorage.setItem(TOKEN_KEY, token)
}

export function clearAccessToken() {
  localStorage.removeItem(TOKEN_KEY)
}

function unwrap<T>(payload: ApiResponse<T>): T {
  if (!payload.success || payload.data == null) {
    throw new Error(payload.message ?? 'The request could not be completed.')
  }

  return payload.data
}

function messageFromAxios(error: unknown): string {
  if (error instanceof AxiosError) {
    const payload = error.response?.data as ApiResponse<unknown> | undefined
    if (payload?.message) {
      return payload.message
    }

    if (error.response?.status === 401) {
      return 'Sign in to continue. Import requires a broker admin or manager account.'
    }

    if (error.response?.status === 403) {
      return 'You do not have permission to import. Ask a broker admin or manager.'
    }
  }

  if (error instanceof Error) {
    return error.message
  }

  return 'The request could not be completed.'
}

/**
 * GET helper that unwraps the JSON envelope's data property.
 * DateOnly fields from the API arrive as "yyyy-MM-dd" strings, not Date objects.
 */
export async function getApiData<T>(url: string): Promise<T> {
  try {
    const response = await http.get<ApiResponse<T>>(url)
    return unwrap(response.data)
  } catch (error) {
    throw new Error(messageFromAxios(error))
  }
}

/** POST JSON helper. Same envelope unwrap as getApiData. */
export async function postApiData<T>(url: string, body: unknown): Promise<T> {
  try {
    const response = await http.post<ApiResponse<T>>(url, body)
    return unwrap(response.data)
  } catch (error) {
    throw new Error(messageFromAxios(error))
  }
}

/**
 * POST multipart helper for import preview/confirm-from-file.
 * Timeout is 2 minutes so a 300-row workbook does not look frozen.
 */
export async function postFormData<T>(url: string, form: FormData): Promise<T> {
  try {
    const response = await http.post<ApiResponse<T>>(url, form, { timeout: 120000 })
    return unwrap(response.data)
  } catch (error) {
    throw new Error(messageFromAxios(error))
  }
}
