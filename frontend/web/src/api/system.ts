import { getApiData, http, sendApiData } from './client'
import type { DemoResetSummary, SystemStatus } from '../types/api'

export function fetchSystemStatus() {
  return getApiData<SystemStatus>('/api/system/status')
}

export async function fetchHealth(): Promise<string> {
  const response = await http.get('/health', { responseType: 'text' })
  return String(response.data)
}

export function resetDemoData() {
  return sendApiData<DemoResetSummary>('post', '/api/dev/reset-demo-data', {}, 120000)
}
