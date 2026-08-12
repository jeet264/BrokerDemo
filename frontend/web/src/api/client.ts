import axios from 'axios'
import type { ApiResponse } from '../types/api'

const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'

export const http = axios.create({
  baseURL,
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json',
  },
})

export async function getApiData<T>(url: string): Promise<T> {
  const response = await http.get<ApiResponse<T>>(url)
  const payload = response.data

  if (!payload.success || payload.data == null) {
    throw new Error(payload.message ?? 'The request could not be completed.')
  }

  return payload.data
}
