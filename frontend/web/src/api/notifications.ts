import { getApiData } from './client'
import type { OutboundNotification } from '../types/api'

export function fetchNotifications() {
  return getApiData<OutboundNotification[]>('/api/notifications')
}
