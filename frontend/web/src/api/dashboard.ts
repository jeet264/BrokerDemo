import { getApiData } from './client'
import type { Dashboard } from '../types/api'

export function fetchDashboard() {
  return getApiData<Dashboard>('/api/dashboard')
}
