import { getApiData } from './client'
import type { RenewalDashboard } from '../types/api'

export function fetchRenewalDashboard() {
  return getApiData<RenewalDashboard>('/api/dashboard/renewals')
}
