import type { ReactNode } from 'react'

export function EmptyState({
  icon = 'bi-inbox',
  title,
  description,
  action,
}: {
  icon?: string
  title: string
  description?: string
  action?: ReactNode
}) {
  return (
    <div className="empty-state">
      <i className={`bi ${icon}`} aria-hidden />
      <h3>{title}</h3>
      {description ? <p>{description}</p> : null}
      {action}
    </div>
  )
}

export function LoadingBlock({ label }: { label: string }) {
  return (
    <div className="loading-block" role="status" aria-live="polite">
      <span className="spinner-border spinner-border-sm" aria-hidden />
      <span>{label}</span>
    </div>
  )
}

export function ErrorBanner({ children }: { children: ReactNode }) {
  return (
    <div className="alert alert-danger" role="alert">
      {children}
    </div>
  )
}
