import { useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Form, Modal } from 'react-bootstrap'
import { useForm } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { fetchDashboard } from '../../api/dashboard'
import { createFollowUp } from '../../api/renewals'
import { PriorityChip, StatusChip } from '../../components/display/StatusChips'
import { EmptyState, ErrorBanner, LoadingBlock } from '../../components/feedback/PageFeedback'
import { useToast } from '../../components/feedback/ToastProvider'
import { daysRemainingShort, formatDateIn, formatDateTimeIst } from '../../lib/format'
import { formatInr } from '../../lib/money'
import type { DashboardTask, UpcomingRenewal } from '../../types/api'

interface FollowUpForm {
  activityType: string
  description: string
  nextFollowUpLocal: string
}

function greetingForIst(name: string) {
  const hour = Number(
    new Intl.DateTimeFormat('en-GB', {
      timeZone: 'Asia/Kolkata',
      hour: 'numeric',
      hourCycle: 'h23',
    }).format(new Date()),
  )
  const period = hour < 12 ? 'morning' : hour < 17 ? 'afternoon' : 'evening'
  return `Good ${period}, ${name}`
}

function tomorrowLocalInput() {
  const tomorrow = new Date()
  tomorrow.setDate(tomorrow.getDate() + 1)
  tomorrow.setHours(10, 0, 0, 0)
  const pad = (value: number) => String(value).padStart(2, '0')
  return `${tomorrow.getFullYear()}-${pad(tomorrow.getMonth() + 1)}-${pad(tomorrow.getDate())}T${pad(tomorrow.getHours())}:${pad(tomorrow.getMinutes())}`
}

export function DashboardPage() {
  const queryClient = useQueryClient()
  const { showToast } = useToast()
  const [followUp, setFollowUp] = useState<UpcomingRenewal | null>(null)

  const dashboardQuery = useQuery({
    queryKey: ['dashboard'],
    queryFn: fetchDashboard,
  })

  const followUpForm = useForm<FollowUpForm>({
    values: useMemo(
      () => ({
        activityType: 'Call',
        description: followUp ? `Follow up with ${followUp.clientName} on ${followUp.policyNumber}.` : '',
        nextFollowUpLocal: tomorrowLocalInput(),
      }),
      [followUp],
    ),
  })

  const followUpMutation = useMutation({
    mutationFn: (values: FollowUpForm) =>
      createFollowUp(followUp!.renewalPublicId, {
        activityType: values.activityType,
        description: values.description.trim(),
        nextFollowUpAtUtc: values.nextFollowUpLocal ? new Date(values.nextFollowUpLocal).toISOString() : undefined,
        createTask: true,
        taskTitle: followUp ? `Follow up: ${followUp.policyNumber}` : 'Follow up on renewal',
      }),
    onSuccess: () => {
      showToast('Follow-up logged', 'The next action is on the task list.', 'success')
      setFollowUp(null)
      void queryClient.invalidateQueries({ queryKey: ['dashboard'] })
      void queryClient.invalidateQueries({ queryKey: ['renewals'] })
    },
    onError: (error: Error) => showToast('Could not log follow-up', error.message, 'danger'),
  })

  const metrics = dashboardQuery.data
  const upcoming = metrics?.upcomingRenewals ?? []
  const todaysTasks = metrics?.todaysTasks ?? []
  const greeting = greetingForIst(metrics?.currentUserName ?? 'there')
  const overdueCount = metrics?.renewalsOverdue ?? 0

  return (
    <div>
      <div className="page-heading">
        <div>
          <h2>{greeting}</h2>
          <p>Policies expiring, renewals at risk, and the work that needs an owner today.</p>
        </div>
      </div>

      {dashboardQuery.isError && (
        <ErrorBanner>Could not load the dashboard. Check your connection and try again.</ErrorBanner>
      )}

      <div className="metric-grid">
        <article className={`metric-card${overdueCount > 0 ? ' metric-card-overdue' : ''}`}>
          <Link to="/renewals?due=overdue" className="metric-card-link">
            <span className="metric-label">Overdue renewals</span>
            <strong>{metrics?.renewalsOverdue ?? '—'}</strong>
            <span className="metric-hint">Past expiry, still open</span>
          </Link>
        </article>
        <article className="metric-card metric-card-warn">
          <Link to="/renewals?due=dueIn7Days" className="metric-card-link">
            <span className="metric-label">Due in 7 days</span>
            <strong>{metrics?.renewalsDueWithin7Days ?? '—'}</strong>
            <span className="metric-hint">Including today</span>
          </Link>
        </article>
        <article className="metric-card">
          <Link to="/renewals?due=dueIn30Days" className="metric-card-link">
            <span className="metric-label">Due in 30 days</span>
            <strong>{metrics?.renewalsDueWithin30Days ?? '—'}</strong>
            <span className="metric-hint">Current term</span>
          </Link>
        </article>
        <article className="metric-card">
          <Link to="/renewals?due=dueIn30Days" className="metric-card-link">
            <span className="metric-label">Premium at risk</span>
            <strong>{metrics ? formatInr(metrics.premiumAtRisk) : '—'}</strong>
            <span className="metric-hint">Open within 90 days</span>
          </Link>
        </article>
        <article className="metric-card">
          <Link to="/tasks" className="metric-card-link">
            <span className="metric-label">Pending tasks</span>
            <strong>{metrics?.pendingTasks ?? '—'}</strong>
            <span className="metric-hint">Work still open</span>
          </Link>
        </article>
      </div>

      <section className="content-card mt-4">
        <div className="d-flex justify-content-between align-items-start gap-3 mb-3">
          <div>
            <div className="section-kicker">Expiring policies</div>
            <h3 className="h5 mb-1">Upcoming renewals</h3>
            <p className="text-muted mb-0">Start with overdue and critical items, then the nearest expiry.</p>
          </div>
          <Link to="/renewals" className="btn btn-sm btn-outline-secondary">
            View all renewals
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
                  <th>Client</th>
                  <th>Policy</th>
                  <th>Insurer</th>
                  <th className="num">Premium</th>
                  <th>Expiry</th>
                  <th>Days left</th>
                  <th>Status</th>
                  <th>Assigned to</th>
                  <th>Action</th>
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
                      <div className="table-actions">
                        <Link to={`/renewals/${renewal.renewalPublicId}`} className="btn btn-sm btn-outline-secondary">
                          View
                        </Link>
                        <Button size="sm" className="btn-gold" onClick={() => setFollowUp(renewal)}>
                          Follow up
                        </Button>
                      </div>
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
            <div className="section-kicker">Required today</div>
            <h3 className="h5 mb-1">Today's tasks</h3>
            <p className="text-muted mb-0">Overdue and due today — the work to clear before close of business.</p>
          </div>
          <Link to="/tasks" className="btn btn-sm btn-outline-secondary">
            View all tasks
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
                  <th>Task</th>
                  <th>Client / policy</th>
                  <th>Due</th>
                  <th>Priority</th>
                  <th>Assigned to</th>
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

      <Modal show={followUp != null} onHide={() => setFollowUp(null)} centered>
        <Form onSubmit={followUpForm.handleSubmit((values) => followUpMutation.mutate(values))}>
          <Modal.Header closeButton>
            <Modal.Title>Follow up</Modal.Title>
          </Modal.Header>
          <Modal.Body>
            <p className="text-muted">
              Log the call or note for {followUp?.clientName} ({followUp?.policyNumber}). A task is created for the next
              step.
            </p>
            <Form.Group className="mb-3">
              <Form.Label>Type</Form.Label>
              <Form.Select {...followUpForm.register('activityType', { required: true })}>
                <option value="Call">Call</option>
                <option value="Email">Email</option>
                <option value="WhatsApp">WhatsApp</option>
                <option value="Note">Note</option>
                <option value="ClientContact">Client contact</option>
                <option value="InsurerContact">Insurer contact</option>
              </Form.Select>
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>What happened</Form.Label>
              <Form.Control as="textarea" rows={3} {...followUpForm.register('description', { required: true })} />
            </Form.Group>
            <Form.Group>
              <Form.Label>Next follow-up</Form.Label>
              <Form.Control type="datetime-local" {...followUpForm.register('nextFollowUpLocal')} />
            </Form.Group>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="outline-secondary" onClick={() => setFollowUp(null)}>
              Cancel
            </Button>
            <Button className="btn-gold" type="submit" disabled={followUpMutation.isPending}>
              Save follow-up
            </Button>
          </Modal.Footer>
        </Form>
      </Modal>
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
    </tr>
  )
}
