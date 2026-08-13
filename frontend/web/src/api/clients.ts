import { getApiData, sendApiData } from './client'
import type {
  ClientActivity,
  ClientDetails,
  ClientListItem,
  ClientPolicy,
  ClientRenewal,
  CreateClientRequest,
  PagedResult,
} from '../types/api'

export function fetchClients(params: {
  search?: string
  clientType?: string
  industry?: string
  assignedUserPublicId?: string
  isActive?: string
  page?: number
  pageSize?: number
} = {}) {
  const search = new URLSearchParams()
  if (params.search) {
    search.set('search', params.search)
  }
  if (params.clientType) {
    search.set('clientType', params.clientType)
  }
  if (params.industry) {
    search.set('industry', params.industry)
  }
  if (params.assignedUserPublicId) {
    search.set('assignedUserPublicId', params.assignedUserPublicId)
  }
  if (params.isActive) {
    search.set('isActive', params.isActive)
  }
  search.set('page', String(params.page ?? 1))
  search.set('pageSize', String(params.pageSize ?? 20))
  search.set('sortBy', 'companyName')
  return getApiData<PagedResult<ClientListItem>>(`/api/clients?${search.toString()}`)
}

export function fetchClient(publicId: string) {
  return getApiData<ClientDetails>(`/api/clients/${publicId}`)
}

export function createClient(body: CreateClientRequest) {
  return sendApiData<ClientDetails>('post', '/api/clients', body)
}

export function fetchClientPolicies(publicId: string) {
  return getApiData<ClientPolicy[]>(`/api/clients/${publicId}/policies`)
}

export function fetchClientRenewals(publicId: string) {
  return getApiData<ClientRenewal[]>(`/api/clients/${publicId}/renewals`)
}

export function fetchClientActivities(publicId: string) {
  return getApiData<ClientActivity[]>(`/api/clients/${publicId}/activities`)
}
