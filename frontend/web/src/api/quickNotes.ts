import { sendApiData } from './client'
import type { QuickNote } from '../types/api'

export function createQuickNote(body: {
  text: string
  clientPublicId?: string
  renewalPublicId?: string
  createFollowUpTask?: boolean
  taskDueDateUtc?: string
}) {
  return sendApiData<QuickNote>('post', '/api/quick-notes', body)
}
