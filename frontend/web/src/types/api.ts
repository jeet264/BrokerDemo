/**
 * Shapes that match the API JSON envelope and system-status payload.
 * Property names are camelCase because ASP.NET uses the default web JSON serializer.
 */
export interface ApiResponse<T> {
  success: boolean
  data: T | null
  message: string | null
  errors: string[] | null
  traceId: string | null
}

export interface SystemStatus {
  productName: string
  tagline: string
  environment: string
  apiVersion: string
  /** ISO-8601 UTC instant from the API. Convert to IST for display; do not treat as a DateOnly cover date. */
  utcNow: string
  /** True when a connection string is configured — not a live SQL ping. */
  databaseConfigured: boolean
}

export interface AuthResponse {
  accessToken: string
  expiresAtUtc: string
  user: {
    publicUserId: string
    email: string
    fullName: string
    role: string
    organizationPublicId: string
    organizationName: string
    organizationCode: string
  }
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface ClientListItem {
  publicId: string
  clientCode: string
  companyName: string
  clientType: string
  industry: string | null
  email: string
  phone: string
  city: string
  state: string
  isActive: boolean
  assignedUserName: string | null
}

export interface ImportPreviewRow<T> {
  rowNumber: number
  isValid: boolean
  error: string | null
  values: T
}

export interface ImportPreview<T> {
  previewToken: string
  totalRows: number
  validCount: number
  invalidCount: number
  matchStrategy: string | null
  rows: ImportPreviewRow<T>[]
}

export interface ClientImportValues {
  clientCode: string | null
  companyName: string | null
  clientType: string | null
  email: string | null
  phone: string | null
  city: string | null
  state: string | null
}

export interface PolicyImportValues {
  policyNumber: string | null
  clientCode: string | null
  clientName: string | null
  phone: string | null
  insurer: string | null
  policyType: string | null
  /** yyyy-MM-dd cover date string from DateOnly — not a Date object. */
  startDate: string | null
  expiryDate: string | null
  premium: string | null
  matchedClientName: string | null
}

export interface ImportSkip {
  rowNumber: number
  reason: string
}

export interface ImportCommitResult {
  importedCount: number
  skippedCount: number
  skipped: ImportSkip[]
}

export type MyDayItemKind = 'Renewal' | 'Task'
export type MyDayBucket = 'Overdue' | 'DueToday' | 'UpcomingUrgent'
export type MyDayActionName = 'Call' | 'MarkDone' | 'SendFollowUp' | 'ViewDetails'

export interface MyDayItem {
  kind: MyDayItemKind
  publicId: string
  clientPublicId: string | null
  clientName: string | null
  clientPhone: string | null
  policyPublicId: string | null
  policyNumber: string | null
  actionNeeded: string
  bucket: MyDayBucket
  /** IST calendar date as yyyy-MM-dd (DateOnly), not a Date object. */
  dueOn: string
  daysOverdue: number | null
  priority: string
  stage: string | null
  availableActions: MyDayActionName[]
}

export interface MyDayBriefing {
  generatedAtUtc: string
  businessDate: string
  overdueItems: MyDayItem[]
  overdueTotalCount: number
  dueTodayItems: MyDayItem[]
  dueTodayTotalCount: number
  upcomingUrgentItems: MyDayItem[]
  upcomingUrgentTotalCount: number
}
