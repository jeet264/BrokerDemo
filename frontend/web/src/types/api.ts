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
  utcNow: string
  databaseConfigured: boolean
}
