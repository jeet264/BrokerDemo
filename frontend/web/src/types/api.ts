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
