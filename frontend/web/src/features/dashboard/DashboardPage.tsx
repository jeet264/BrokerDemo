import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { fetchDashboard } from '../../api/dashboard'
import type { UpcomingRenewal } from '../../types/api'

function formatInr(amount: number) {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    maximumFractionDigits: 0,
  }).format(amount)
}

function daysLabel(daysRemaining: number) {
  if (daysRemaining < 0) {
    return `${Math.abs(daysRemaining)}d overdue`
  }
  if (daysRemaining === 0) {
    return 'Due today'
  }
  return `${daysRemaining}d`
}

function priorityClass(priority: string) {
  switch (priority) {
    case 'Critical':
      return 'text-danger'
    case 'High':
      return 'text-warning'
    default:
      return 'text-muted'
  }
}

function MetricCard({
  label,
  value,
  hint,
  danger,
}: {
  label: string
  value: string | number
  hint: string
  danger?: boolean
}) {
  return (
    <article className="metric-card">
      <span className="metric-label">{label}</span>
      <strong className={danger ? 'text-danger' : undefined}>{value}</strong>
      <span className="metric-hint">{hint}</span>
    </article>
  )
}

export function DashboardPage() {
  const dashboardQuery = useQuery({
    queryKey: ['dashboard'],
    queryFn: fetchDashboard,
  })

  const metrics = dashboardQuery.data
  const upcoming = metrics?.upcomingRenewals ?? []

  return (
    <div>
      <div className="page-heading">
        <div>
          <h2>Dashboard</h2>
          <p>Renewal workload for this organisation. Counts use the current policy term after a rollover, not the expired one.</p>
        </div>
      </div>

      {dashboardQuery.isError && (
        <div className="alert alert-danger" role="alert">
          Could not load the dashboard. Sign in and confirm the API is running.
        </div>
      )}

      <div className="metric-grid">
        <MetricCard label="Total clients" value={metrics?.totalClients ?? '—'} hint="All client records" />
        <MetricCard label="Active policies" value={metrics?.activePolicies ?? '—'} hint="Current-term policies" />
        <MetricCard
          label="Overdue"
          value={metrics?.renewalsOverdue ?? '—'}
          hint="Open renewals past expiry"
          danger={(metrics?.renewalsOverdue ?? 0) > 0}
        />
        <MetricCard label="Due today" value={metrics?.renewalsDueToday ?? '—'} hint="Expires today" />
        <MetricCard label="Due in 7 days" value={metrics?.renewalsDueWithin7Days ?? '—'} hint="Including today" />
        <MetricCard label="Due in 30 days" value={metrics?.renewalsDueWithin30Days ?? '—'} hint="Current term only" />
        <MetricCard label="Due in 60 days" value={metrics?.renewalsDueWithin60Days ?? '—'} hint="Upcoming current terms" />
        <MetricCard
          label="Premium at risk"
          value={metrics ? formatInr(metrics.premiumAtRisk) : '—'}
          hint="Open renewals within 90 days"
        />
        <MetricCard label="Pending tasks" value={metrics?.pendingTasks ?? '—'} hint="Open work items" />
        <MetricCard label="Completed today" value={metrics?.completedTasksToday ?? '—'} hint="Tasks closed today" />
        <MetricCard label="Pending follow-ups" value={metrics?.pendingFollowUps ?? '—'} hint="Scheduled on open renewals" />
      </div>

      <section className="content-card mt-4">
        <div className="d-flex justify-content-between align-items-start gap-3 mb-3">
          <div>
            <h3 className="h5 mb-1">Upcoming renewals</h3>
            <p className="text-muted mb-0">Overdue first, then critical priority, then nearest expiry.</p>
          </div>
          <Link to="/renewals" className="btn btn-sm btn-outline-secondary">
            View all
          </Link>
        </div>
        {dashboardQuery.isLoading && <p className="text-muted mb-0">Loading dashboard…</p>}
        {!dashboardQuery.isLoading && upcoming.length === 0 && (
          <p className="text-muted mb-0">No open renewals in this view.</p>
        )}
        {upcoming.length > 0 && (
          <div className="table-responsive">
            <table className="table align-middle mb-0">
              <thead>
                <tr>
                  <th>Policy</th>
                  <th>Client</th>
                  <th>Insurer</th>
                  <th>Expiry</th>
                  <th>Days</th>
                  <th>Premium</th>
                  <th>Status</th>
                  <th>Priority</th>
                  <th>Assigned</th>
                </tr>
              </thead>
              <tbody>
                {upcoming.map((renewal) => (
                  <UpcomingRow key={renewal.renewalPublicId} renewal={renewal} />
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  )
}

function UpcomingRow({ renewal }: { renewal: UpcomingRenewal }) {
  return (
    <tr className={renewal.daysRemaining < 0 ? 'table-danger' : undefined}>
      <td>
        <strong>{renewal.policyNumber}</strong>
        <div className="text-muted small">{renewal.policyType}</div>
      </td>
      <td>{renewal.clientName}</td>
      <td>{renewal.insurerName}</td>
      <td>{renewal.expiryDate}</td>
      <td className={renewal.daysRemaining <= 0 ? 'text-danger fw-semibold' : undefined}>{daysLabel(renewal.daysRemaining)}</td>
      <td>{formatInr(renewal.premium)}</td>
      <td>{renewal.status}</td>
      <td className={priorityClass(renewal.priority)}>{renewal.priority}</td>
      <td>{renewal.assignedUserName ?? '—'}</td>
    </tr>
  )
}
