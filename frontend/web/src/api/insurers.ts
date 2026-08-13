import { getApiData } from './client'
import type { InsurerListItem } from '../types/api'

export function fetchInsurers() {
  return getApiData<InsurerListItem[]>('/api/insurers')
}
