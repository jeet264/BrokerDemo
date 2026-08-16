import { getApiData, sendApiData } from './client'
import type { PagedResult, PolicyDetails, PolicyListItem, UpsertPolicyRequest } from '../types/api'

export function fetchPolicies(params: {
  search?: string
  status?: string
  policyType?: string
  insurerPublicId?: string
  assignedUserPublicId?: string
  clientPublicId?: string
  fromDate?: string
  toDate?: string
  page?: number
  pageSize?: number
} = {}) {
  const search = new URLSearchParams()
  if (params.search) {
    search.set('search', params.search)
  }
  if (params.status) {
    search.set('status', params.status)
  }
  if (params.policyType) {
    search.set('policyType', params.policyType)
  }
  if (params.insurerPublicId) {
    search.set('insurerPublicId', params.insurerPublicId)
  }
  if (params.assignedUserPublicId) {
    search.set('assignedUserPublicId', params.assignedUserPublicId)
  }
  if (params.clientPublicId) {
    search.set('clientPublicId', params.clientPublicId)
  }
  if (params.fromDate) {
    search.set('fromDate', params.fromDate)
  }
  if (params.toDate) {
    search.set('toDate', params.toDate)
  }
  search.set('page', String(params.page ?? 1))
  search.set('pageSize', String(params.pageSize ?? 50))
  search.set('sortBy', 'expiryDate')
  return getApiData<PagedResult<PolicyListItem>>(`/api/policies?${search.toString()}`)
}

export function fetchPolicy(publicId: string) {
  return getApiData<PolicyDetails>(`/api/policies/${publicId}`)
}

export function createPolicy(body: UpsertPolicyRequest) {
  return sendApiData<PolicyDetails>('post', '/api/policies', body)
}

export function updatePolicy(publicId: string, body: UpsertPolicyRequest) {
  return sendApiData<PolicyDetails>('put', `/api/policies/${publicId}`, body)
}
