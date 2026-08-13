import { getApiData } from './client'
import type { TeamUser } from '../types/api'

export function fetchUsers() {
  return getApiData<TeamUser[]>('/api/users')
}
