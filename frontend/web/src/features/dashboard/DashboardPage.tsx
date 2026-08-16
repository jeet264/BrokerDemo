import { useQuery } from '@tanstack/react-query'
import { fetchHealth, fetchSystemStatus } from '../../api/system'

/**
 * Formats an API UTC ISO timestamp for display in India Standard Time.
 * Gotcha: pass the string from SystemStatus.utcNow, not a DateOnly cover date (those are yyyy-MM-dd without a timezone).
 */
function formatIst(utcIso: string) {
  return new Intl.DateTimeFormat('en-IN', {
    dateStyle: 'medium',
    timeStyle: 'short',
    timeZone: 'Asia/Kolkata',
  }).format(new Date(utcIso))
}

/**
 * Home dashboard. Currently shows API liveness and environment, not renewal KPIs
 * (those arrive when policy/renewal list APIs exist).
 *
 * Does not require a JWT — GET /api/system/status and GET /health are anonymous.
 */
export function DashboardPage() {
  const statusQuery = useQuery({
    queryKey: ['system-status'],
    queryFn: fetchSystemStatus,
  })
  const healthQuery = useQuery({
    queryKey: ['health'],
    queryFn: fetchHealth,
  })

  const status = statusQuery.data
  const apiOnline = statusQuery.isSuccess && healthQuery.isSuccess

  return (
    <div>
      <div className="page-heading">
        <div>
          <h2>Overview</h2>
          <p>Environment and connectivity. Your next calls live on My Day — this page is the system pulse, not the to-do list.</p>
        </div>
      </div>

      <div className="metric-grid">
        <article className="metric-card">
          <span className="metric-label">API</span>
          <strong className={apiOnline ? 'text-success' : 'text-danger'}>{apiOnline ? 'Online' : statusQuery.isLoading ? 'Checking' : 'Offline'}</strong>
          <span className="metric-hint">{healthQuery.data ?? 'Waiting for /health'}</span>
        </article>
        <article className="metric-card">
          <span className="metric-label">Environment</span>
          <strong>{status?.environment ?? '—'}</strong>
          <span className="metric-hint">API {status?.apiVersion ?? 'not connected'}</span>
        </article>
        <article className="metric-card">
          <span className="metric-label">India time</span>
          <strong>{status ? formatIst(status.utcNow) : '—'}</strong>
          <span className="metric-hint">Stored timestamps use UTC</span>
        </article>
        <article className="metric-card">
          <span className="metric-label">Database</span>
          <strong>{status?.databaseConfigured ? 'Configured' : 'Not connected'}</strong>
          <span className="metric-hint">SQL Server wiring starts in Phase 2</span>
        </article>
      </div>

      {statusQuery.isError && (
        <div className="alert alert-danger mt-4" role="alert">
          Could not reach the BrokerOS API. Start the API on port 5000 and refresh.
        </div>
      )}

      <section className="content-card mt-4">
        <h3>What this demo will show</h3>
        <ul className="checklist">
          <li>Policies approaching expiry</li>
          <li>Overdue renewals that need a call today</li>
          <li>Clients waiting on follow-up</li>
          <li>The next action on each renewal</li>
        </ul>
      </section>
    </div>
  )
}
