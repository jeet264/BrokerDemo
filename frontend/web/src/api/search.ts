import { getApiData } from './client'
import type { SearchResults } from '../types/api'

export function fetchSearch(query: string) {
  const search = new URLSearchParams()
  search.set('q', query)
  return getApiData<SearchResults>(`/api/search?${search.toString()}`)
}
