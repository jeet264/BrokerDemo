import axios from 'axios'
import type { ApiResponse } from '../types/api'

/**
 * Axios instance for the BrokerOS API.
 * Base URL: VITE_API_BASE_URL, or http://localhost:5000 in local dev.
 * JSON bodies use the envelope { success, data, message, errors, traceId }.
 */
const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'

export const http = axios.create({
  baseURL,
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json',
  },
})

/**
 * GET helper that unwraps the JSON envelope's data property.
 * Throws when success is false or data is null — callers should not read the envelope themselves.
 * DateOnly fields from the API arrive as "yyyy-MM-dd" strings, not Date objects.
 */
export async function getApiData<T>(url: string): Promise<T> {
  const response = await http.get<ApiResponse<T>>(url)
  const payload = response.data

  if (!payload.success || payload.data == null) {
    throw new Error(payload.message ?? 'The request could not be completed.')
  }

  return payload.data
}
