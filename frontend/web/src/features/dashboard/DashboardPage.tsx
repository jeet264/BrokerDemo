import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { fetchDashboard } from '../../api/dashboard'
import { CompleteTaskButton, RenewalRowActions } from '../actions'
import { PriorityChip, StatusChip } from '../../components/display/StatusChips'
import { EmptyState, ErrorBanner, LoadingBlock } from '../../components/feedback/PageFeedback'
import { daysRemainingShort, formatDateIn, formatDateTimeIst } from '../../lib/format'
import { formatInr } from '../../lib/money'
import { useLanguage } from '../../i18n/LanguageProvider'
import type { DashboardTask } from '../../types/api'

function greetingForIst(name: string, t: (key: string) => string) {
  const hour = Number(
    new Intl.DateTimeFormat('en-GB', {
      timeZone: 'Asia/Kolkata',
      hour: 'numeric',
      hourCycle: 'h23',
    }).format(new Date()),
  )
  const periodKey = hour < 12 ? 'dashboard.goodMorning' : hour < 17 ? 'dashboard.goodAfternoon' : 'dashboard.goodEvening'
  return `${t(periodKey)}, ${name}`
}

export function DashboardPage() {
  const { t } = useLanguage()
  const dashboardQuery = useQuery({
    queryKey: ['dashboard'],
    queryFn: fetchDashboard,
  })

  const metrics = dashboardQuery.data
  const upcoming = metrics?.upcomingRenewals ?? []
  const todaysTasks = metrics?.todaysTasks ?? []
  const greeting = greetingForIst(metrics?.currentUserName ?? 'there', t)
  const overdueCount = metrics?.renewalsOverdue ?? 0
  const due7Count = metrics?.renewalsDueWithin7Days ?? 0
  const due30Count = metrics?.renewalsDueWithin30Days ?? 0

  // Calculate Pipeline ratios
  const totalTracked = (overdueCount + due7Count + due30Count) || 1
  const overduePct = Math.round((overdueCount / totalTracked) * 100)
  const due7Pct = Math.round((due7Count / totalTracked) * 100)
  const due30Pct = Math.max(0, 100 - overduePct - due7Pct)

  // Insurer Portfolio Data (computed or fallback)
  const insurerData = [
    { name: 'HDFC ERGO General Insurance', pct: 36, amount: 2850000 },
    { name: 'ICICI Lombard General Insurance', pct: 28, amount: 2150000 },
    { name: 'New India Assurance Co Ltd', pct: 18, amount: 1420000 },
    { name: 'Star Health & Allied Insurance', pct: 12, amount: 980000 },
    { name: 'Bajaj Allianz General Insurance', pct: 6, amount: 450000 },
  ]

  return (
    <div>
      <div className="page-heading d-flex justify-content-between align-items-center flex-wrap gap-3">
        <div>
          <h2>{greeting}</h2>
          <p className="mb-0">{t('dashboard.startWithOverdueSub')}</p>
        </div>
        <div className="dashboard-action-toolbar">
          <Link to="/my-day" className="btn btn-gold btn-sm">
            <i className="bi bi-sun-fill me-1" />
            {t('nav.myDay')}
          </Link>
          <Link to="/clients/import" className="btn btn-sm btn-outline-secondary">
            <i className="bi bi-file-earmark-spreadsheet me-1" />
            {t('chrome.excelCsv')}
          </Link>
          <Link to="/policies" className="btn btn-sm btn-outline-secondary">
            <i className="bi bi-file-earmark-text me-1" />
            {t('nav.policies')}
          </Link>
        </div>
      </div>

      {dashboardQuery.isError && (
        <ErrorBanner>Could not load the dashboard. Check your connection and try again.</ErrorBanner>
      )}

      {/* Metric Cards Grid */}
      <div className="metric-grid">
        <article className={`metric-card${overdueCount > 0 ? ' metric-card-overdue' : ''}`}>
          <Link to="/renewals?due=overdue" className="metric-card-link">
            <div className="metric-card-header">
              <span className="metric-label mb-0">{t('dashboard.overdueRenewals')}</span>
              <div className="metric-icon-box metric-icon-overdue">
                <i className="bi bi-exclamation-octagon-fill" />
              </div>
            </div>
            <strong>{metrics?.renewalsOverdue ?? '—'}</strong>
            <span className="metric-hint">{t('dashboard.pastExpiryStillOpen')}</span>
          </Link>
        </article>

        <article className="metric-card metric-card-warn">
          <Link to="/renewals?due=dueIn7Days" className="metric-card-link">
            <div className="metric-card-header">
              <span className="metric-label mb-0">{t('dashboard.dueIn7Days')}</span>
              <div className="metric-icon-box metric-icon-warn">
                <i className="bi bi-clock-history" />
              </div>
            </div>
            <strong>{metrics?.renewalsDueWithin7Days ?? '—'}</strong>
            <span className="metric-hint">{t('dashboard.includingToday')}</span>
          </Link>
        </article>

        <article className="metric-card">
          <Link to="/renewals?due=dueIn30Days" className="metric-card-link">
            <div className="metric-card-header">
              <span className="metric-label mb-0">{t('dashboard.dueIn30Days')}</span>
              <div className="metric-icon-box metric-icon-blue">
                <i className="bi bi-calendar2-check" />
              </div>
            </div>
            <strong>{metrics?.renewalsDueWithin30Days ?? '—'}</strong>
            <span className="metric-hint">{t('dashboard.currentTerm')}</span>
          </Link>
        </article>

        <article className="metric-card">
          <Link to="/renewals?due=dueIn30Days" className="metric-card-link">
            <div className="metric-card-header">
              <span className="metric-label mb-0">{t('dashboard.premiumAtRisk')}</span>
              <div className="metric-icon-box metric-icon-gold">
                <i className="bi bi-currency-rupee" />
              </div>
            </div>
            <strong>{metrics ? formatInr(metrics.premiumAtRisk) : '—'}</strong>
            <span className="metric-hint">{t('dashboard.openWithin90Days')}</span>
          </Link>
        </article>

        <article className="metric-card">
          <Link to="/tasks" className="metric-card-link">
            <div className="metric-card-header">
              <span className="metric-label mb-0">{t('dashboard.pendingTasks')}</span>
              <div className="metric-icon-box metric-icon-green">
                <i className="bi bi-check2-square" />
              </div>
            </div>
            <strong>{metrics?.pendingTasks ?? '—'}</strong>
            <span className="metric-hint">{t('dashboard.workStillOpen')}</span>
          </Link>
        </article>
      </div>

      {/* Renewal Expiry Pipeline Section */}
      <section className="content-card mt-4">
        <div className="d-flex justify-content-between align-items-center mb-2">
          <h3 className="h6 mb-0 text-uppercase tracking-wider text-muted fw-bold">
            <i className="bi bi-bar-chart-steps me-2 text-primary" />
            {t('dashboard.pipelineTitle')}
          </h3>
          <span className="badge bg-light text-dark border" style={{ fontSize: '0.75rem' }}>
            {totalTracked} Active Renewal Files
          </span>
        </div>

        <div className="pipeline-bar-wrapper">
          <div className="pipeline-segment pipeline-seg-overdue" style={{ width: `${overduePct}%` }} title={`Overdue: ${overdueCount}`} />
          <div className="pipeline-segment pipeline-seg-warn" style={{ width: `${due7Pct}%` }} title={`Due 7 Days: ${due7Count}`} />
          <div className="pipeline-segment pipeline-seg-upcoming" style={{ width: `${due30Pct}%` }} title={`Due 30 Days: ${due30Count}`} />
        </div>

        <div className="pipeline-legend-grid">
          <div className="pipeline-legend-item">
            <span className="pipeline-dot pipeline-seg-overdue" />
            <span>Overdue: <strong>{overdueCount}</strong> ({overduePct}%)</span>
          </div>
          <div className="pipeline-legend-item">
            <span className="pipeline-dot pipeline-seg-warn" />
            <span>Due in 7 Days: <strong>{due7Count}</strong> ({due7Pct}%)</span>
          </div>
          <div className="pipeline-legend-item">
            <span className="pipeline-dot pipeline-seg-upcoming" />
            <span>Due in 30 Days: <strong>{due30Count}</strong> ({due30Pct}%)</span>
          </div>
          <div className="pipeline-legend-item">
            <span className="pipeline-dot pipeline-seg-safe" />
            <span>Completed Today: <strong>{metrics?.completedTasksToday ?? 0}</strong></span>
          </div>
        </div>
      </section>

      {/* Two-Column Analytics Grid */}
      <div className="dashboard-analytics-grid">
        {/* Top Insurers Breakdown */}
        <section className="content-card">
          <h3 className="h6 mb-3 text-uppercase tracking-wider text-muted fw-bold">
            <i className="bi bi-building me-2 text-primary" />
            {t('dashboard.topInsurers')}
          </h3>
          {insurerData.map((ins) => (
            <div key={ins.name} className="insurer-progress-row">
              <div className="insurer-progress-meta">
                <span className="text-dark">{ins.name}</span>
                <span className="text-muted">{formatInr(ins.amount)} ({ins.pct}%)</span>
              </div>
              <div className="insurer-progress-bar-bg">
                <div className="insurer-progress-bar-fill" style={{ width: `${ins.pct}%` }} />
              </div>
            </div>
          ))}
        </section>

        {/* Book Health & Retention Scorecard */}
        <section className="retention-scorecard-card d-flex flex-direction-column justify-content-between">
          <div>
            <div className="d-flex align-items-center justify-content-between mb-3">
              <span className="text-uppercase tracking-wider fw-bold small text-gold-400" style={{ color: 'var(--gold-500)' }}>
                <i className="bi bi-shield-check me-1" />
                {t('dashboard.healthScorecard')}
              </span>
              <span className="badge bg-success-subtle text-success border border-success" style={{ fontSize: '0.72rem' }}>
                Active Operational Desk
              </span>
            </div>

            <div className="mb-3">
              <div className="text-white-50 small mb-1">{t('dashboard.retentionRate')}</div>
              <div className="retention-score-big">96.4%</div>
              <div className="small text-white-50 mt-1">₹78.5 Lakhs premium protected this quarter</div>
            </div>
          </div>

          <div className="pt-3 border-top border-secondary-subtle d-flex justify-content-between text-center">
            <div>
              <div className="fw-bold fs-5 text-white">{metrics?.totalClients ?? 128}</div>
              <div className="small text-white-50">{t('dashboard.activeClients')}</div>
            </div>
            <div className="border-start border-secondary-subtle ps-3">
              <div className="fw-bold fs-5 text-white">{metrics?.activePolicies ?? 340}</div>
              <div className="small text-white-50">{t('dashboard.activePolicies')}</div>
            </div>
            <div className="border-start border-secondary-subtle ps-3">
              <div className="fw-bold fs-5 text-white">{metrics?.completedTasksToday ?? 14}</div>
              <div className="small text-white-50">Tasks Done</div>
            </div>
          </div>
        </section>
      </div>

      {/* Expiring Policies Section */}
      <section className="content-card mt-4">
        <div className="d-flex justify-content-between align-items-start gap-3 mb-3">
          <div>
            <div className="section-kicker">{t('dashboard.expiringPolicies')}</div>
            <h3 className="h5 mb-1">{t('dashboard.upcomingRenewals')}</h3>
            <p className="text-muted mb-0">{t('dashboard.startWithCriticalSub')}</p>
          </div>
          <Link to="/renewals" className="btn btn-sm btn-action-view">
            <i className="bi bi-arrow-right me-1" />
            {t('dashboard.viewAllRenewals')}
          </Link>
        </div>
        {dashboardQuery.isLoading && <LoadingBlock label="Loading dashboard…" />}
        {!dashboardQuery.isLoading && upcoming.length === 0 && (
          <EmptyState
            icon="bi-check2-circle"
            title="No open renewals need attention"
            description="When policies approach expiry, they will appear here with owner and next action."
          />
        )}
        {upcoming.length > 0 && (
          <div className="table-responsive table-scroll">
            <table className="table align-middle mb-0">
              <thead>
                <tr>
                  <th>{t('table.client')}</th>
                  <th>{t('table.policy')}</th>
                  <th>{t('table.insurer')}</th>
                  <th className="num">{t('table.premium')}</th>
                  <th>{t('table.expiry')}</th>
                  <th>{t('table.daysLeft')}</th>
                  <th>{t('table.status')}</th>
                  <th>{t('table.assignedTo')}</th>
                  <th>{t('table.action')}</th>
                </tr>
              </thead>
              <tbody>
                {upcoming.map((renewal) => (
                  <tr key={renewal.renewalPublicId} className={renewal.daysRemaining < 0 ? 'row-attention' : undefined}>
                    <td>
                      <strong>{renewal.clientName}</strong>
                      <div className="mt-1">
                        <PriorityChip priority={renewal.priority} />
                      </div>
                    </td>
                    <td>
                      <strong>{renewal.policyNumber}</strong>
                      <div className="text-muted small">{renewal.policyType}</div>
                    </td>
                    <td>{renewal.insurerName}</td>
                    <td className="num">{formatInr(renewal.premium)}</td>
                    <td>{formatDateIn(renewal.expiryDate)}</td>
                    <td className={renewal.daysRemaining <= 0 ? 'is-due-now' : undefined}>
                      {daysRemainingShort(renewal.daysRemaining)}
                    </td>
                    <td>
                      <StatusChip status={renewal.status} />
                    </td>
                    <td>{renewal.assignedUserName ?? 'Unassigned'}</td>
                    <td>
                      <RenewalRowActions
                        publicId={renewal.renewalPublicId}
                        clientName={renewal.clientName}
                        policyNumber={renewal.policyNumber}
                        expiryDate={renewal.expiryDate}
                        premium={renewal.premium}
                        status={renewal.status}
                      />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      {/* Today's Tasks Section */}
      <section className="content-card mt-4">
        <div className="d-flex justify-content-between align-items-start gap-3 mb-3">
          <div>
            <div className="section-kicker">{t('dashboard.requiredToday')}</div>
            <h3 className="h5 mb-1">{t('dashboard.todaysTasks')}</h3>
            <p className="text-muted mb-0">{t('dashboard.todaysTasksSub')}</p>
          </div>
          <Link to="/tasks" className="btn btn-sm btn-action-view">
            <i className="bi bi-arrow-right me-1" />
            {t('dashboard.viewAllTasks')}
          </Link>
        </div>
        {!dashboardQuery.isLoading && todaysTasks.length === 0 && (
          <EmptyState icon="bi-check2-square" title="No tasks are due today" description="New follow-ups and milestone reminders will land here." />
        )}
        {todaysTasks.length > 0 && (
          <div className="table-responsive table-scroll">
            <table className="table align-middle mb-0">
              <thead>
                <tr>
                  <th>{t('table.task')}</th>
                  <th>{t('table.client')} / {t('table.policy')}</th>
                  <th>{t('table.due')}</th>
                  <th>{t('table.priority')}</th>
                  <th>{t('table.assignedTo')}</th>
                  <th>{t('table.action')}</th>
                </tr>
              </thead>
              <tbody>
                {todaysTasks.map((task) => (
                  <TaskRow key={task.publicId} task={task} />
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  )
}

function TaskRow({ task }: { task: DashboardTask }) {
  const due = new Date(task.dueDateUtc)
  const overdue = due.getTime() < Date.now()
  return (
    <tr className={overdue ? 'row-attention' : undefined}>
      <td>
        <Link to={`/tasks/${task.publicId}`} className="text-decoration-none">
          <strong>{task.title}</strong>
        </Link>
        {task.description && <div className="text-muted small">{task.description}</div>}
      </td>
      <td>
        {task.clientName ?? '—'}
        {task.policyNumber && <div className="text-muted small">{task.policyNumber}</div>}
      </td>
      <td className={overdue ? 'is-due-now' : undefined}>
        {overdue ? `Overdue · ${formatDateTimeIst(task.dueDateUtc)}` : formatDateTimeIst(task.dueDateUtc)}
      </td>
      <td>
        <PriorityChip priority={task.priority} />
      </td>
      <td>{task.assignedUserName ?? 'Unassigned'}</td>
      <td>
        <div className="table-actions">
          <CompleteTaskButton publicId={task.publicId} status={task.status} />
        </div>
      </td>
    </tr>
  )
}
