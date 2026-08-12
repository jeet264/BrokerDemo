import { getApiData } from './client'
import type { PagedResult, PolicyListItem } from '../types/api'

export function fetchPolicies(status = 'Active') {
  const search = new URLSearchParams({
    status,
    pageSize: '50',
    sortBy: 'expiryDate',
  })
  return getApiData<PagedResult<PolicyListItem>>(`/api/policies?${search.toString()}`)
}
