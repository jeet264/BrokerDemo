import { useQuery } from '@tanstack/react-query'
import { fetchHealth, fetchSystemStatus } from '../../api/system'
import { fetchRenewalDashboard } from '../../api/dashboard'

function formatIst(utcIso: string) {
  return new Intl.DateTimeFormat('en-IN', {
    dateStyle: 'medium',
    timeStyle: 'short',
    timeZone: 'Asia/Kolkata',
  }).format(new Date(utcIso))
}

function formatInr(amount: number) {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    maximumFractionDigits: 0,
  }).format(amount)
}

export function DashboardPage() {
  const statusQuery = useQuery({
    queryKey: ['system-status'],
    queryFn: fetchSystemStatus,
  })
  const healthQuery = useQuery({
    queryKey: ['health'],
    queryFn: fetchHealth,
  })
  const dashboardQuery = useQuery({
    queryKey: ['renewal-dashboard'],
    queryFn: fetchRenewalDashboard,
  })

  const status = statusQuery.data
  const metrics = dashboardQuery.data
  const apiOnline = statusQuery.isSuccess && healthQuery.isSuccess

  return (
    <div>
      <div className="page-heading">
        <div>
          <h2>Dashboard</h2>
          <p>Never miss an insurance renewal. Counts use the current policy term after a rollover, not the expired one.</p>
        </div>
      </div>

      <div className="metric-grid">
        <article className="metric-card">
          <span className="metric-label">Overdue</span>
          <strong className="text-danger">{metrics?.overdue ?? '—'}</strong>
          <span className="metric-hint">Open renewals past expiry</span>
        </article>
        <article className="metric-card">
          <span className="metric-label">Due today</span>
          <strong>{metrics?.dueToday ?? '—'}</strong>
          <span className="metric-hint">Current term expires today</span>
        </article>
        <article className="metric-card">
          <span className="metric-label">Due in 7 days</span>
          <strong>{metrics?.dueWithin7Days ?? '—'}</strong>
          <span className="metric-hint">Including today</span>
        </article>
        <article className="metric-card">
          <span className="metric-label">Due in 30 days</span>
          <strong>{metrics?.dueWithin30Days ?? '—'}</strong>
          <span className="metric-hint">Current term only</span>
        </article>
        <article className="metric-card">
          <span className="metric-label">Due in 60 days</span>
          <strong>{metrics?.dueWithin60Days ?? '—'}</strong>
          <span className="metric-hint">Upcoming current terms</span>
        </article>
        <article className="metric-card">
          <span className="metric-label">Renewed</span>
          <strong className="text-success">{metrics?.renewed ?? '—'}</strong>
          <span className="metric-hint">Rolled to a new term</span>
        </article>
        <article className="metric-card">
          <span className="metric-label">Lost</span>
          <strong>{metrics?.lost ?? '—'}</strong>
          <span className="metric-hint">Policy cancelled</span>
        </article>
        <article className="metric-card">
          <span className="metric-label">Premium at risk</span>
          <strong>{metrics ? formatInr(metrics.premiumAtRisk) : '—'}</strong>
          <span className="metric-hint">Open renewals within 90 days</span>
        </article>
      </div>

      <div className="metric-grid mt-3">
        <article className="metric-card">
          <span className="metric-label">API</span>
          <strong className={apiOnline ? 'text-success' : 'text-danger'}>{apiOnline ? 'Online' : statusQuery.isLoading ? 'Checking' : 'Offline'}</strong>
          <span className="metric-hint">{healthQuery.data ?? 'Waiting for /health'}</span>
        </article>
        <article className="metric-card">
          <span className="metric-label">India time</span>
          <strong>{status ? formatIst(status.utcNow) : '—'}</strong>
          <span className="metric-hint">Stored timestamps use UTC</span>
        </article>
      </div>

      {(statusQuery.isError || dashboardQuery.isError) && (
        <div className="alert alert-danger mt-4" role="alert">
          Could not reach the BrokerOS API. Sign in, start the API on port 5000, and refresh.
        </div>
      )}
    </div>
  )
}
