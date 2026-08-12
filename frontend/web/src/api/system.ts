import { http, getApiData } from './client'
import type { SystemStatus } from '../types/api'

export function fetchSystemStatus() {
  return getApiData<SystemStatus>('/api/system/status')
}

export async function fetchHealth(): Promise<string> {
  const response = await http.get('/health', { responseType: 'text' })
  return String(response.data)
}
