import { getApiData, sendApiData } from './client'
import type { PagedResult, UpdateWorkTaskRequest, WorkTaskDetails, WorkTaskListItem } from '../types/api'

export type TaskView = 'mine' | 'team' | 'overdue' | 'completed'

export function fetchTasks(params: {
  view?: TaskView
  status?: string
  priority?: string
  assignedUserPublicId?: string
  fromDate?: string
  toDate?: string
  page?: number
  pageSize?: number
} = {}) {
  const search = new URLSearchParams()
  if (params.view) {
    search.set('view', params.view)
  }
  if (params.status) {
    search.set('status', params.status)
  }
  if (params.priority) {
    search.set('priority', params.priority)
  }
  if (params.assignedUserPublicId) {
    search.set('assignedUserPublicId', params.assignedUserPublicId)
  }
  if (params.fromDate) {
    search.set('fromDate', params.fromDate)
  }
  if (params.toDate) {
    search.set('toDate', params.toDate)
  }
  search.set('page', String(params.page ?? 1))
  search.set('pageSize', String(params.pageSize ?? 50))
  search.set('sortBy', 'dueDateUtc')
  return getApiData<PagedResult<WorkTaskListItem>>(`/api/tasks?${search.toString()}`)
}

export function fetchTask(publicId: string) {
  return getApiData<WorkTaskDetails>(`/api/tasks/${publicId}`)
}

export function updateTask(publicId: string, body: UpdateWorkTaskRequest) {
  return sendApiData<WorkTaskDetails>('put', `/api/tasks/${publicId}`, body)
}

export function completeTask(publicId: string) {
  return sendApiData<WorkTaskDetails>('put', `/api/tasks/${publicId}/complete`)
}

export function reassignTask(publicId: string, assignedUserPublicId: string) {
  return sendApiData<WorkTaskDetails>('put', `/api/tasks/${publicId}/reassign`, { assignedUserPublicId })
}

export function cancelTask(publicId: string) {
  return sendApiData<WorkTaskDetails>('put', `/api/tasks/${publicId}/cancel`)
}
