import { getApiData, sendApiData } from './client'
import type { OutboundNotification, Quotation } from '../types/api'

export interface QuotationWriteBody {
  insurerPublicId?: string
  newInsurerName?: string
  premiumAmount: number
  sumInsured?: number | null
  coverageSummary?: string
  validUntil?: string | null
  notes?: string | null
}

export function fetchRenewalQuotations(renewalPublicId: string) {
  return getApiData<Quotation[]>(`/api/renewals/${renewalPublicId}/quotations`)
}

export function createQuotation(renewalPublicId: string, body: QuotationWriteBody) {
  return sendApiData<Quotation>('post', `/api/renewals/${renewalPublicId}/quotations`, body)
}

export function updateQuotation(publicId: string, body: QuotationWriteBody) {
  return sendApiData<Quotation>('put', `/api/quotations/${publicId}`, body)
}

export function selectQuotation(publicId: string) {
  return sendApiData<Quotation>('put', `/api/quotations/${publicId}/select`)
}

export function deleteQuotation(publicId: string) {
  return sendApiData<unknown>('delete', `/api/quotations/${publicId}`)
}

export function shareQuotation(publicId: string) {
  return sendApiData<OutboundNotification>('post', `/api/quotations/${publicId}/share`)
}

export function shareQuotationComparison(renewalPublicId: string) {
  return sendApiData<OutboundNotification>('post', `/api/renewals/${renewalPublicId}/quotations/compare-share`)
}
