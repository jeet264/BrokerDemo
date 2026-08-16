import { http, getApiData } from './client'
import type { SystemStatus } from '../types/api'

/** GET /api/system/status → SystemStatusDto inside the success envelope (product, env, utcNow, databaseConfigured). */
export function fetchSystemStatus() {
  return getApiData<SystemStatus>('/api/system/status')
}

/**
 * GET /health — ASP.NET health check, plain text ("Healthy"), not the JSON envelope.
 * Used only as a liveness ping on the dashboard.
 */
export async function fetchHealth(): Promise<string> {
  const response = await http.get('/health', { responseType: 'text' })
  return String(response.data)
}
