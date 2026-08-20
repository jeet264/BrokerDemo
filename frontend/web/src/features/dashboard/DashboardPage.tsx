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

  return (
    <div>
      <div className="page-heading">
        <div>
          <h2>{greeting}</h2>
          <p>{t('dashboard.startWithOverdueSub')}</p>
        </div>
      </div>

      {dashboardQuery.isError && (
        <ErrorBanner>Could not load the dashboard. Check your connection and try again.</ErrorBanner>
      )}

      <div className="metric-grid">
        <article className={`metric-card${overdueCount > 0 ? ' metric-card-overdue' : ''}`}>
          <Link to="/renewals?due=overdue" className="metric-card-link">
            <span className="metric-label">{t('dashboard.overdueRenewals')}</span>
            <strong>{metrics?.renewalsOverdue ?? '—'}</strong>
            <span className="metric-hint">{t('dashboard.pastExpiryStillOpen')}</span>
          </Link>
        </article>
        <article className="metric-card metric-card-warn">
          <Link to="/renewals?due=dueIn7Days" className="metric-card-link">
            <span className="metric-label">{t('dashboard.dueIn7Days')}</span>
            <strong>{metrics?.renewalsDueWithin7Days ?? '—'}</strong>
            <span className="metric-hint">{t('dashboard.includingToday')}</span>
          </Link>
        </article>
        <article className="metric-card">
          <Link to="/renewals?due=dueIn30Days" className="metric-card-link">
            <span className="metric-label">{t('dashboard.dueIn30Days')}</span>
            <strong>{metrics?.renewalsDueWithin30Days ?? '—'}</strong>
            <span className="metric-hint">{t('dashboard.currentTerm')}</span>
          </Link>
        </article>
        <article className="metric-card">
          <Link to="/renewals?due=dueIn30Days" className="metric-card-link">
            <span className="metric-label">{t('dashboard.premiumAtRisk')}</span>
            <strong>{metrics ? formatInr(metrics.premiumAtRisk) : '—'}</strong>
            <span className="metric-hint">{t('dashboard.openWithin90Days')}</span>
          </Link>
        </article>
        <article className="metric-card">
          <Link to="/tasks" className="metric-card-link">
            <span className="metric-label">{t('dashboard.pendingTasks')}</span>
            <strong>{metrics?.pendingTasks ?? '—'}</strong>
            <span className="metric-hint">{t('dashboard.workStillOpen')}</span>
          </Link>
        </article>
      </div>

      <section className="content-card mt-4">
        <div className="d-flex justify-content-between align-items-start gap-3 mb-3">
          <div>
            <div className="section-kicker">{t('dashboard.expiringPolicies')}</div>
            <h3 className="h5 mb-1">{t('dashboard.upcomingRenewals')}</h3>
            <p className="text-muted mb-0">{t('dashboard.startWithCriticalSub')}</p>
          </div>
          <Link to="/renewals" className="btn btn-sm btn-outline-secondary">
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

      <section className="content-card mt-4">
        <div className="d-flex justify-content-between align-items-start gap-3 mb-3">
          <div>
            <div className="section-kicker">{t('dashboard.requiredToday')}</div>
            <h3 className="h5 mb-1">{t('dashboard.todaysTasks')}</h3>
            <p className="text-muted mb-0">{t('dashboard.todaysTasksSub')}</p>
          </div>
          <Link to="/tasks" className="btn btn-sm btn-outline-secondary">
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
