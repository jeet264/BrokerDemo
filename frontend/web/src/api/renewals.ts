import { getApiData, sendApiData } from './client'
import type { PagedResult, RenewalListItem } from '../types/api'

export function fetchRenewals(params: { status?: string; pageSize?: number } = {}) {
  const search = new URLSearchParams()
  if (params.status) {
    search.set('status', params.status)
  }
  search.set('pageSize', String(params.pageSize ?? 50))
  search.set('sortBy', 'renewalDate')
  return getApiData<PagedResult<RenewalListItem>>(`/api/renewals?${search.toString()}`)
}

export function completeRenewal(
  publicId: string,
  body: { newExpiryDate: string; premium: number },
) {
  return sendApiData<RenewalListItem>('put', `/api/renewals/${publicId}/complete`, body)
}

export function createFollowUp(
  publicId: string,
  body: {
    activityType: string
    description: string
    nextFollowUpAtUtc?: string
    createTask?: boolean
    taskTitle?: string
  },
) {
  return sendApiData<RenewalListItem>('post', `/api/renewals/${publicId}/follow-up`, body)
}

export function markRenewalLost(publicId: string, reason?: string) {
  return sendApiData<RenewalListItem>('put', `/api/renewals/${publicId}/lost`, { reason })
}
