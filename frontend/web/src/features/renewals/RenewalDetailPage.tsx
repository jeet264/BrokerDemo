import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Form, Modal } from 'react-bootstrap'
import { useForm } from 'react-hook-form'
import { Link, useParams } from 'react-router-dom'
import { applyApiFieldErrors } from '../../api/client'
import {
  createFollowUp,
  createRenewalTask,
  fetchRenewal,
  fetchRenewalNotifications,
  fetchRenewalTasks,
  updateRenewalStage,
} from '../../api/renewals'
import { AddFollowUpModal, CompleteTaskButton, MarkLostModal, MarkRenewedModal } from '../actions'
import { PriorityChip, StatusChip } from '../../components/display/StatusChips'
import { EmptyState, ErrorBanner, LoadingBlock } from '../../components/feedback/PageFeedback'
import { useToast } from '../../components/feedback/ToastProvider'
import { formatDateIn, formatDateTimeIst, initials, urgencyFromDays } from '../../lib/format'
import { formatInr } from '../../lib/money'
import type { OutboundNotification, RenewalDetails, RenewalTask } from '../../types/api'
import { NotificationPreviewModal } from '../notifications/NotificationPreviewModal'
import { SIMULATION_BADGE, channelLabel, recipientTypeLabel } from '../notifications/notificationDisplay'
import {
  activityTitle,
  daysRemainingCopy,
  formatExpiryLong,
  formatIst,
  isOpenRenewal,
  istDateToUtc,
  nextRequiredAction,
  RENEWAL_STAGES,
  stageLabel,
  tomorrowIsoDate,
} from './renewalDisplay'

type ActionModal = 'contact' | 'followUp' | 'task' | 'stage' | 'renew' | 'lost' | null

interface ContactForm {
  description: string
  nextFollowUpDate: string
}

interface TaskForm {
  title: string
  description: string
  dueDate: string
  priority: string
}

interface StageForm {
  stage: string
  notes: string
}

export function RenewalDetailPage() {
  const { publicId = '' } = useParams()
  const queryClient = useQueryClient()
  const { showToast } = useToast()
  const [action, setAction] = useState<ActionModal>(null)

  const renewalQuery = useQuery({
    queryKey: ['renewal', publicId],
    queryFn: () => fetchRenewal(publicId),
    enabled: Boolean(publicId),
  })

  const notificationsQuery = useQuery({
    queryKey: ['renewal-notifications', publicId],
    queryFn: () => fetchRenewalNotifications(publicId),
    enabled: Boolean(publicId),
  })
  const tasksQuery = useQuery({
    queryKey: ['renewal-tasks', publicId],
    queryFn: () => fetchRenewalTasks(publicId),
    enabled: Boolean(publicId),
  })
  const [preview, setPreview] = useState<OutboundNotification | null>(null)

  const renewal = renewalQuery.data
  const open = renewal ? isOpenRenewal(renewal.status) : false

  const refreshFrom = (updated: RenewalDetails) => {
    queryClient.setQueryData(['renewal', publicId], updated)
    void queryClient.invalidateQueries({ queryKey: ['renewals'] })
    void queryClient.invalidateQueries({ queryKey: ['renewal-tasks', publicId] })
    void queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    void queryClient.invalidateQueries({ queryKey: ['policies'] })
    void queryClient.invalidateQueries({ queryKey: ['clients'] })
    void queryClient.invalidateQueries({ queryKey: ['tasks'] })
  }

  if (renewalQuery.isError) {
    return (
      <div>
        <div className="page-heading">
          <Link to="/renewals" className="text-decoration-none">
            ← Renewals
          </Link>
          <h2 className="mt-2">Renewal not found</h2>
        </div>
        <ErrorBanner>This renewal is not in your book, or the API could not be reached.</ErrorBanner>
      </div>
    )
  }

  if (!renewal) {
    return (
      <div>
        <div className="page-heading">
          <h2>Renewal</h2>
        </div>
        <LoadingBlock label="Loading renewal…" />
      </div>
    )
  }

  const next = nextRequiredAction(renewal)
  const nextActionId = next.action
  const urgency = urgencyFromDays(renewal.daysRemaining)
  const openTasks = (tasksQuery.data ?? []).filter((task) => task.status !== 'Completed' && task.status !== 'Cancelled')

  return (
    <div>
      <div className="page-heading">
        <Link to="/renewals" className="text-decoration-none">
          ← Renewals
        </Link>
        <h2 className="mt-2 mb-1">{renewal.clientName}</h2>
        <p className="mb-0">
          <Link to={`/policies/${renewal.policyPublicId}`}>{renewal.policyNumber}</Link>
          <span className="text-muted">
            {' '}
            · {renewal.policyType} · {renewal.insurerName}
          </span>
        </p>
      </div>

      <section className={`content-card expiry-hero expiry-hero-${urgency}`}>
        <div className="row g-3 align-items-end">
          <div className="col-lg-8">
            <div className="section-kicker">Policy expiry</div>
            <div className="expiry-hero-date">{formatExpiryLong(renewal.expiryDate)}</div>
            <div className={`expiry-hero-days${renewal.daysRemaining <= 0 ? ' is-due-now' : ''}`}>
              {daysRemainingCopy(renewal.daysRemaining)}
            </div>
            <div className="expiry-hero-meta">
              <PriorityChip priority={renewal.priority} />
              <StatusChip status={renewal.status} />
              <span className="num">{formatInr(renewal.premium)}</span>
              <span>Stage · {stageLabel(renewal.currentStage)}</span>
            </div>
          </div>
          <div className="col-lg-4">
            <div className="owner-block">
              <span className="user-avatar">{initials(next.owner)}</span>
              <div>
                <div className="owner-label">Owner</div>
                <div className="owner-name">{next.owner}</div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section className="content-card next-action-card mt-4">
        <div className="d-flex justify-content-between align-items-start flex-wrap gap-3">
          <div>
            <div className="section-kicker">Required action</div>
            <h3 className="next-action-title">{next.title}</h3>
            <p className="text-muted mb-0">{next.detail}</p>
          </div>
          {open && nextActionId && (
            <Button className="btn-gold" onClick={() => setAction(nextActionId)}>
              {next.cta}
            </Button>
          )}
        </div>
      </section>

      <div className="row g-4 mt-1">
        <div className="col-lg-8">
          <section className="content-card">
            <div className="section-kicker">Progress</div>
            <h3 className="h6 mb-0">Renewal stage</h3>
            <ol className="stage-progress">
              {RENEWAL_STAGES.map((stage, index) => {
                const currentIndex = RENEWAL_STAGES.findIndex((item) => item.id === renewal.currentStage)
                const state =
                  index < currentIndex ? 'is-done' : index === currentIndex ? 'is-current' : 'is-upcoming'
                return (
                  <li key={stage.id} className={state}>
                    <span className="stage-progress-marker">{index + 1}</span>
                    <span className="stage-progress-label">{stage.label}</span>
                  </li>
                )
              })}
            </ol>
          </section>

          <section className="content-card mt-4">
            <div className="section-kicker">What should happen next</div>
            <h3 className="h6 mb-3">Open tasks</h3>
            {tasksQuery.isLoading && <LoadingBlock label="Loading tasks…" />}
            {!tasksQuery.isLoading && openTasks.length === 0 && (
              <EmptyState
                icon="bi-check2-square"
                title="No open tasks on this file"
                description="Create a task if someone needs to follow up, or log contact to keep the timeline current."
              />
            )}
            {openTasks.length > 0 && (
              <div className="table-responsive">
                <table className="table align-middle mb-0">
                  <thead>
                    <tr>
                      <th>Task</th>
                      <th>Due</th>
                      <th>Priority</th>
                      <th>Owner</th>
                      <th>Action</th>
                    </tr>
                  </thead>
                  <tbody>
                    {openTasks.map((task) => (
                      <OpenTaskRow key={task.publicId} task={task} />
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </section>

          <section className="content-card mt-4">
            <div className="section-kicker">What happened previously</div>
            <h3 className="h6 mb-3">Timeline</h3>
            {renewal.activities.length === 0 && (
              <EmptyState icon="bi-clock-history" title="No activity recorded yet" description="Contact, follow-ups, and stage changes will appear here." />
            )}
            {renewal.activities.length > 0 && (
              <ol className="activity-timeline">
                {renewal.activities.map((activity) => (
                  <li key={activity.publicId}>
                    <strong>{activityTitle(activity.activityType, activity.description)}</strong>
                    <div>{activity.description}</div>
                    <div className="text-muted small">
                      {formatIst(activity.createdAtUtc)}
                      {activity.userName ? ` · ${activity.userName}` : ''}
                    </div>
                  </li>
                ))}
              </ol>
            )}
          </section>

          <section className="content-card mt-4">
            <div className="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-3">
              <div>
                <div className="section-kicker">Reminders</div>
                <h3 className="h6 mb-0">Notifications</h3>
              </div>
              <span className="sim-badge">{SIMULATION_BADGE}</span>
            </div>
            {notificationsQuery.isError && <ErrorBanner>Could not load simulated notifications.</ErrorBanner>}
            {notificationsQuery.isLoading && <LoadingBlock label="Loading notifications…" />}
            {!notificationsQuery.isLoading && (notificationsQuery.data?.length ?? 0) === 0 && (
              <EmptyState
                icon="bi-chat-dots"
                title="No simulated notifications yet"
                description="The renewal worker drafts WhatsApp client reminders at 90/60/45/30/15/7/1-day milestones. Nothing is actually sent."
              />
            )}
            {(notificationsQuery.data?.length ?? 0) > 0 && (
              <div className="table-responsive">
                <table className="table align-middle mb-0">
                  <thead>
                    <tr>
                      <th>Channel</th>
                      <th>To</th>
                      <th>Subject</th>
                      <th>Created</th>
                      <th />
                    </tr>
                  </thead>
                  <tbody>
                    {notificationsQuery.data!.map((notification) => (
                      <tr key={notification.publicId}>
                        <td>
                          <strong>{channelLabel(notification.channel)}</strong>
                          <div className="text-muted small">{recipientTypeLabel(notification.recipientType)}</div>
                        </td>
                        <td>
                          {notification.recipientName}
                          {notification.recipientAddress && (
                            <div className="text-muted small">{notification.recipientAddress}</div>
                          )}
                        </td>
                        <td>{notification.subject}</td>
                        <td>{formatIst(notification.createdAtUtc)}</td>
                        <td>
                          <button
                            type="button"
                            className="btn btn-sm btn-outline-secondary"
                            onClick={() => setPreview(notification)}
                          >
                            Preview
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </section>
        </div>

        <div className="col-lg-4">
          <section className="content-card">
            <div className="section-kicker">Desk</div>
            <h3 className="h6 text-uppercase text-muted">Actions</h3>
            {open ? (
              <div className="renewal-actions">
                <Button className="btn-gold" onClick={() => setAction('contact')}>
                  Contact Client
                </Button>
                <Button variant="outline-secondary" onClick={() => setAction('followUp')}>
                  Add Follow-up
                </Button>
                <Button variant="outline-secondary" onClick={() => setAction('task')}>
                  Create Task
                </Button>
                <Button variant="outline-secondary" onClick={() => setAction('stage')}>
                  Change Stage
                </Button>
                <Button className="btn-gold" onClick={() => setAction('renew')}>
                  Mark Renewed
                </Button>
                <Button variant="outline-danger" onClick={() => setAction('lost')}>
                  Mark Lost
                </Button>
              </div>
            ) : (
              <p className="text-muted mb-0">
                This renewal is {renewal.status.toLowerCase()}. Timeline remains for the record.
                {renewal.nextPolicyNumber ? ` Next term is ${renewal.nextPolicyNumber}.` : ''}
              </p>
            )}
            <dl className="detail-list mt-4 mb-0">
              <div>
                <dt>Client</dt>
                <dd>
                  <Link to={`/clients/${renewal.clientPublicId}`}>{renewal.clientName}</Link>
                </dd>
              </div>
              <div>
                <dt>Insurer</dt>
                <dd>{renewal.insurerName}</dd>
              </div>
              <div>
                <dt>Premium</dt>
                <dd>{formatInr(renewal.premium)}</dd>
              </div>
              <div>
                <dt>Sum insured</dt>
                <dd>{formatInr(renewal.sumInsured)}</dd>
              </div>
              <div>
                <dt>Start</dt>
                <dd>{formatDateIn(renewal.startDate)}</dd>
              </div>
              <div>
                <dt>Last follow-up</dt>
                <dd>{formatDateTimeIst(renewal.lastFollowUpAtUtc)}</dd>
              </div>
              <div>
                <dt>Next follow-up</dt>
                <dd>{formatDateTimeIst(renewal.nextFollowUpAtUtc)}</dd>
              </div>
              <div>
                <dt>Assigned</dt>
                <dd>{renewal.assignedUserName ?? 'Unassigned'}</dd>
              </div>
              {renewal.nextPolicyNumber && (
                <div>
                  <dt>Next term</dt>
                  <dd>
                    {renewal.nextPolicyPublicId ? (
                      <Link to={`/policies/${renewal.nextPolicyPublicId}`}>{renewal.nextPolicyNumber}</Link>
                    ) : (
                      renewal.nextPolicyNumber
                    )}
                    {renewal.nextPolicyExpiryDate ? ` · ${formatDateIn(renewal.nextPolicyExpiryDate)}` : ''}
                  </dd>
                </div>
              )}
            </dl>
          </section>
        </div>
      </div>

      <ContactClientModal
        show={action === 'contact'}
        publicId={publicId}
        onHide={() => setAction(null)}
        onSaved={(updated) => {
          refreshFrom(updated)
          setAction(null)
          showToast('Client contacted', 'The timeline has been updated.', 'success')
        }}
      />
      <AddFollowUpModal
        show={action === 'followUp'}
        publicId={publicId}
        onHide={() => setAction(null)}
      />
      <CreateTaskModal
        show={action === 'task'}
        publicId={publicId}
        onHide={() => setAction(null)}
        onSaved={(updated) => {
          refreshFrom(updated)
          setAction(null)
          showToast('Task created', 'The timeline has been updated.', 'success')
        }}
      />
      <ChangeStageModal
        show={action === 'stage'}
        publicId={publicId}
        currentStage={renewal.currentStage}
        onHide={() => setAction(null)}
        onSaved={(updated) => {
          refreshFrom(updated)
          setAction(null)
          showToast('Stage updated', stageLabel(updated.currentStage), 'success')
        }}
      />
      <MarkRenewedModal
        show={action === 'renew'}
        publicId={publicId}
        expiryDate={renewal.expiryDate}
        premium={renewal.premium}
        onHide={() => setAction(null)}
      />
      <MarkLostModal
        show={action === 'lost'}
        publicId={publicId}
        policyNumber={renewal.policyNumber}
        onHide={() => setAction(null)}
      />
      <NotificationPreviewModal notification={preview} onHide={() => setPreview(null)} />
    </div>
  )
}

function OpenTaskRow({ task }: { task: RenewalTask }) {
  const overdue = new Date(task.dueDateUtc).getTime() < Date.now() && task.status !== 'Completed'
  return (
    <tr className={overdue ? 'row-attention' : undefined}>
      <td>
        <Link to={`/tasks/${task.publicId}`} className="text-decoration-none">
          <strong>{task.title}</strong>
        </Link>
        {task.description && <div className="text-muted small">{task.description}</div>}
      </td>
      <td className={overdue ? 'is-due-now' : undefined}>{formatDateTimeIst(task.dueDateUtc)}</td>
      <td>
        <PriorityChip priority={task.priority} />
      </td>
      <td>{task.assignedUserName ?? 'Unassigned'}</td>
      <td>
        <CompleteTaskButton publicId={task.publicId} status={task.status} />
      </td>
    </tr>
  )
}

function ContactClientModal({
  show,
  publicId,
  onHide,
  onSaved,
}: {
  show: boolean
  publicId: string
  onHide: () => void
  onSaved: (updated: RenewalDetails) => void
}) {
  const { showToast } = useToast()
  const form = useForm<ContactForm>({
    values: { description: '', nextFollowUpDate: tomorrowIsoDate() },
  })
  const mutation = useMutation({
    mutationFn: (values: ContactForm) =>
      createFollowUp(publicId, {
        activityType: 'ClientContact',
        description: values.description.trim(),
        nextFollowUpAtUtc: values.nextFollowUpDate ? istDateToUtc(values.nextFollowUpDate) : undefined,
      }),
    onSuccess: onSaved,
    onError: (error: Error) => {
      applyApiFieldErrors(error, form.setError)
      showToast('Could not log contact', error.message, 'danger')
    },
  })

  return (
    <Modal show={show} onHide={onHide} centered>
      <Form onSubmit={form.handleSubmit((values) => mutation.mutate(values))}>
        <Modal.Header closeButton>
          <Modal.Title>Contact client</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form.Group className="mb-3">
            <Form.Label>What was discussed</Form.Label>
            <Form.Control
              as="textarea"
              rows={3}
              isInvalid={Boolean(form.formState.errors.description)}
              {...form.register('description', { required: 'Description is required' })}
            />
            <Form.Control.Feedback type="invalid">{form.formState.errors.description?.message}</Form.Control.Feedback>
          </Form.Group>
          <Form.Group>
            <Form.Label>Next follow-up date</Form.Label>
            <Form.Control type="date" {...form.register('nextFollowUpDate')} />
          </Form.Group>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="outline-secondary" onClick={onHide}>
            Cancel
          </Button>
          <Button className="btn-gold" type="submit" disabled={mutation.isPending}>
            Save contact
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  )
}

function CreateTaskModal({
  show,
  publicId,
  onHide,
  onSaved,
}: {
  show: boolean
  publicId: string
  onHide: () => void
  onSaved: (updated: RenewalDetails) => void
}) {
  const { showToast } = useToast()
  const form = useForm<TaskForm>({
    values: { title: 'Follow up on renewal', description: '', dueDate: tomorrowIsoDate(), priority: 'Medium' },
  })
  const mutation = useMutation({
    mutationFn: (values: TaskForm) =>
      createRenewalTask(publicId, {
        title: values.title.trim(),
        description: values.description.trim() || undefined,
        dueDateUtc: istDateToUtc(values.dueDate),
        priority: values.priority,
      }),
    onSuccess: onSaved,
    onError: (error: Error) => {
      applyApiFieldErrors(error, form.setError)
      showToast('Could not create task', error.message, 'danger')
    },
  })

  return (
    <Modal show={show} onHide={onHide} centered>
      <Form onSubmit={form.handleSubmit((values) => mutation.mutate(values))}>
        <Modal.Header closeButton>
          <Modal.Title>Create task</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form.Group className="mb-3">
            <Form.Label>Title</Form.Label>
            <Form.Control
              isInvalid={Boolean(form.formState.errors.title)}
              {...form.register('title', { required: 'Title is required', maxLength: 200 })}
            />
            <Form.Control.Feedback type="invalid">{form.formState.errors.title?.message}</Form.Control.Feedback>
          </Form.Group>
          <Form.Group className="mb-3">
            <Form.Label>Description</Form.Label>
            <Form.Control as="textarea" rows={3} {...form.register('description')} />
          </Form.Group>
          <Form.Group className="mb-3">
            <Form.Label>Due date</Form.Label>
            <Form.Control type="date" {...form.register('dueDate', { required: 'Due date is required' })} />
          </Form.Group>
          <Form.Group>
            <Form.Label>Priority</Form.Label>
            <Form.Select {...form.register('priority')}>
              <option value="Low">Low</option>
              <option value="Medium">Medium</option>
              <option value="High">High</option>
              <option value="Critical">Critical</option>
            </Form.Select>
          </Form.Group>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="outline-secondary" onClick={onHide}>
            Cancel
          </Button>
          <Button className="btn-gold" type="submit" disabled={mutation.isPending}>
            Create task
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  )
}

function ChangeStageModal({
  show,
  publicId,
  currentStage,
  onHide,
  onSaved,
}: {
  show: boolean
  publicId: string
  currentStage: string
  onHide: () => void
  onSaved: (updated: RenewalDetails) => void
}) {
  const { showToast } = useToast()
  const form = useForm<StageForm>({
    values: { stage: currentStage === 'Completed' ? 'ClientDecision' : currentStage, notes: '' },
  })
  const mutation = useMutation({
    mutationFn: (values: StageForm) =>
      updateRenewalStage(publicId, { stage: values.stage, notes: values.notes.trim() || undefined }),
    onSuccess: onSaved,
    onError: (error: Error) => {
      applyApiFieldErrors(error, form.setError)
      showToast('Could not change stage', error.message, 'danger')
    },
  })

  return (
    <Modal show={show} onHide={onHide} centered>
      <Form onSubmit={form.handleSubmit((values) => mutation.mutate(values))}>
        <Modal.Header closeButton>
          <Modal.Title>Change stage</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form.Group className="mb-3" controlId="renewal-stage">
            <Form.Label>Stage</Form.Label>
            <Form.Select {...form.register('stage', { required: true })}>
              {RENEWAL_STAGES.filter((stage) => stage.id !== 'Completed').map((stage) => (
                <option key={stage.id} value={stage.id}>
                  {stage.label}
                </option>
              ))}
            </Form.Select>
            <div className="form-text">Mark Renewed moves the file to Completed and rolls the policy term.</div>
          </Form.Group>
          <Form.Group>
            <Form.Label>Notes</Form.Label>
            <Form.Control as="textarea" rows={3} {...form.register('notes')} />
          </Form.Group>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="outline-secondary" onClick={onHide}>
            Cancel
          </Button>
          <Button className="btn-gold" type="submit" disabled={mutation.isPending}>
            Save stage
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  )
}
