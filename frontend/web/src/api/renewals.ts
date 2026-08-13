import { getApiData, sendApiData } from './client'
import type { OutboundNotification, PagedResult, RenewalDetails, RenewalListItem, RenewalTask } from '../types/api'

export type RenewalDueFilter =
  | 'all'
  | 'overdue'
  | 'dueToday'
  | 'dueIn7Days'
  | 'dueIn30Days'
  | 'completed'
  | 'lost'

export function fetchRenewals(params: { dueFilter?: RenewalDueFilter; search?: string; pageSize?: number } = {}) {
  const search = new URLSearchParams()
  if (params.dueFilter) {
    search.set('dueFilter', params.dueFilter)
  }
  if (params.search) {
    search.set('search', params.search)
  }
  search.set('pageSize', String(params.pageSize ?? 50))
  search.set('sortBy', 'renewalDate')
  return getApiData<PagedResult<RenewalListItem>>(`/api/renewals?${search.toString()}`)
}

export function fetchRenewalTasks(publicId: string) {
  return getApiData<RenewalTask[]>(`/api/renewals/${publicId}/tasks`)
}

export function fetchRenewal(publicId: string) {
  return getApiData<RenewalDetails>(`/api/renewals/${publicId}`)
}

export function updateRenewalStage(publicId: string, body: { stage: string; notes?: string }) {
  return sendApiData<RenewalDetails>('put', `/api/renewals/${publicId}/stage`, body)
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
  return sendApiData<RenewalDetails>('post', `/api/renewals/${publicId}/follow-up`, body)
}

export function createRenewalTask(
  publicId: string,
  body: { title: string; description?: string; dueDateUtc: string; priority: string },
) {
  return sendApiData<RenewalDetails>('post', `/api/renewals/${publicId}/tasks`, body)
}

export function completeRenewal(
  publicId: string,
  body: { newExpiryDate: string; premium: number },
) {
  return sendApiData<RenewalDetails>('put', `/api/renewals/${publicId}/complete`, body)
}

export function markRenewalLost(publicId: string, reason?: string) {
  return sendApiData<RenewalDetails>('put', `/api/renewals/${publicId}/lost`, { reason })
}

export function fetchRenewalNotifications(publicId: string) {
  return getApiData<OutboundNotification[]>(`/api/renewals/${publicId}/notifications`)
}
