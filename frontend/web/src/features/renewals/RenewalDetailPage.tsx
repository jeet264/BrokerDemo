import { useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Form, Modal } from 'react-bootstrap'
import { useForm } from 'react-hook-form'
import { Link, useParams } from 'react-router-dom'
import { applyApiFieldErrors } from '../../api/client'
import {
  completeRenewal,
  createFollowUp,
  createRenewalTask,
  fetchRenewal,
  markRenewalLost,
  updateRenewalStage,
} from '../../api/renewals'
import { useToast } from '../../components/feedback/ToastProvider'
import { formatInr } from '../../lib/money'
import type { RenewalDetails } from '../../types/api'
import {
  activityTitle,
  addDays,
  addYears,
  daysRemainingCopy,
  FOLLOW_UP_TYPES,
  formatExpiryLong,
  formatIst,
  isOpenRenewal,
  istDateToUtc,
  priorityClass,
  RENEWAL_STAGES,
  stageLabel,
  tomorrowIsoDate,
} from './renewalDisplay'

type ActionModal = 'contact' | 'followUp' | 'task' | 'stage' | 'renew' | 'lost' | null

interface FollowUpForm {
  activityType: string
  description: string
  nextFollowUpDate: string
}

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

interface RenewForm {
  newExpiryDate: string
  premium: number
}

interface LostForm {
  reason: string
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

  const renewal = renewalQuery.data
  const open = renewal ? isOpenRenewal(renewal.status) : false

  const refreshFrom = (updated: RenewalDetails) => {
    queryClient.setQueryData(['renewal', publicId], updated)
    void queryClient.invalidateQueries({ queryKey: ['renewals'] })
    void queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    void queryClient.invalidateQueries({ queryKey: ['policies'] })
    void queryClient.invalidateQueries({ queryKey: ['clients'] })
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
        <div className="alert alert-danger">This renewal is not in your book, or the API could not be reached.</div>
      </div>
    )
  }

  if (!renewal) {
    return (
      <div>
        <div className="page-heading">
          <h2>Renewal</h2>
          <p className="text-muted">Loading renewal…</p>
        </div>
      </div>
    )
  }

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

      <div className="row g-4">
        <div className="col-lg-8">
          <section className="content-card expiry-hero">
            <div className="expiry-hero-date">{formatExpiryLong(renewal.expiryDate)}</div>
            <div className={`expiry-hero-days${renewal.daysRemaining <= 0 ? ' is-due-now' : ''}`}>
              {daysRemainingCopy(renewal.daysRemaining)}
            </div>
            <div className="expiry-hero-meta">
              <span className={priorityClass(renewal.priority)}>{renewal.priority}</span>
              <span>{renewal.status}</span>
              <span>{formatInr(renewal.premium)}</span>
              <span>{renewal.assignedUserName ?? 'Unassigned'}</span>
            </div>
          </section>

          <section className="content-card mt-4">
            <h3 className="h6 text-uppercase text-muted">Renewal stage</h3>
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
            <h3 className="h6 text-uppercase text-muted">Timeline</h3>
            {renewal.activities.length === 0 && <p className="text-muted mb-0">No activity recorded yet.</p>}
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
        </div>

        <div className="col-lg-4">
          <section className="content-card">
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
                <dt>Assigned</dt>
                <dd>{renewal.assignedUserName ?? 'Unassigned'}</dd>
              </div>
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
      <FollowUpModal
        show={action === 'followUp'}
        publicId={publicId}
        onHide={() => setAction(null)}
        onSaved={(updated) => {
          refreshFrom(updated)
          setAction(null)
          showToast('Follow-up logged', 'The timeline has been updated.', 'success')
        }}
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
        onSaved={(updated) => {
          refreshFrom(updated)
          setAction(null)
          showToast(
            'Policy renewed',
            `Next term ${updated.nextPolicyNumber ?? updated.policyNumber} expires ${updated.nextPolicyExpiryDate ?? updated.expiryDate}.`,
            'success',
          )
        }}
      />
      <MarkLostModal
        show={action === 'lost'}
        publicId={publicId}
        policyNumber={renewal.policyNumber}
        onHide={() => setAction(null)}
        onSaved={(updated) => {
          refreshFrom(updated)
          setAction(null)
          showToast('Marked lost', 'The policy was cancelled. No new term was created.', 'info')
        }}
      />
    </div>
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

function FollowUpModal({
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
  const form = useForm<FollowUpForm>({
    values: { activityType: 'Call', description: '', nextFollowUpDate: tomorrowIsoDate() },
  })
  const mutation = useMutation({
    mutationFn: (values: FollowUpForm) =>
      createFollowUp(publicId, {
        activityType: values.activityType,
        description: values.description.trim(),
        nextFollowUpAtUtc: values.nextFollowUpDate ? istDateToUtc(values.nextFollowUpDate) : undefined,
      }),
    onSuccess: onSaved,
    onError: (error: Error) => {
      applyApiFieldErrors(error, form.setError)
      showToast('Could not log follow-up', error.message, 'danger')
    },
  })

  return (
    <Modal show={show} onHide={onHide} centered>
      <Form onSubmit={form.handleSubmit((values) => mutation.mutate(values))}>
        <Modal.Header closeButton>
          <Modal.Title>Add follow-up</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form.Group className="mb-3">
            <Form.Label>Follow-up type</Form.Label>
            <Form.Select {...form.register('activityType', { required: 'Follow-up type is required' })}>
              {FOLLOW_UP_TYPES.map((type) => (
                <option key={type.id} value={type.id}>
                  {type.label}
                </option>
              ))}
            </Form.Select>
          </Form.Group>
          <Form.Group className="mb-3">
            <Form.Label>Description</Form.Label>
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
            Save follow-up
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
          <Form.Group className="mb-3">
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

function MarkRenewedModal({
  show,
  publicId,
  expiryDate,
  premium,
  onHide,
  onSaved,
}: {
  show: boolean
  publicId: string
  expiryDate: string
  premium: number
  onHide: () => void
  onSaved: (updated: RenewalDetails) => void
}) {
  const { showToast } = useToast()
  const defaults = useMemo<RenewForm>(() => {
    const nextStart = addDays(expiryDate, 1)
    return { newExpiryDate: addYears(nextStart, 1), premium }
  }, [expiryDate, premium])
  const form = useForm<RenewForm>({ values: defaults })
  const mutation = useMutation({
    mutationFn: (values: RenewForm) => completeRenewal(publicId, values),
    onSuccess: onSaved,
    onError: (error: Error) => {
      applyApiFieldErrors(error, form.setError)
      showToast('Could not renew', error.message, 'danger')
    },
  })

  return (
    <Modal show={show} onHide={onHide} centered>
      <Form onSubmit={form.handleSubmit((values) => mutation.mutate(values))}>
        <Modal.Header closeButton>
          <Modal.Title>Mark renewed</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <p className="text-muted">Creates the next-term policy starting the day after the current expiry. The old policy is kept as Expired.</p>
          <Form.Group className="mb-3">
            <Form.Label>New expiry date</Form.Label>
            <Form.Control type="date" {...form.register('newExpiryDate', { required: true })} />
          </Form.Group>
          <Form.Group>
            <Form.Label>Premium</Form.Label>
            <Form.Control type="number" step="0.01" min="0" {...form.register('premium', { required: true, valueAsNumber: true })} />
          </Form.Group>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="outline-secondary" onClick={onHide}>
            Cancel
          </Button>
          <Button className="btn-gold" type="submit" disabled={mutation.isPending}>
            Confirm
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  )
}

function MarkLostModal({
  show,
  publicId,
  policyNumber,
  onHide,
  onSaved,
}: {
  show: boolean
  publicId: string
  policyNumber: string
  onHide: () => void
  onSaved: (updated: RenewalDetails) => void
}) {
  const { showToast } = useToast()
  const form = useForm<LostForm>({ values: { reason: '' } })
  const mutation = useMutation({
    mutationFn: (values: LostForm) => markRenewalLost(publicId, values.reason.trim() || undefined),
    onSuccess: onSaved,
    onError: (error: Error) => {
      applyApiFieldErrors(error, form.setError)
      showToast('Could not mark lost', error.message, 'danger')
    },
  })

  return (
    <Modal show={show} onHide={onHide} centered>
      <Form onSubmit={form.handleSubmit((values) => mutation.mutate(values))}>
        <Modal.Header closeButton>
          <Modal.Title>Mark lost</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <p>Cancel {policyNumber}? The policy will be marked Cancelled and no new term will be created.</p>
          <Form.Group>
            <Form.Label>Reason</Form.Label>
            <Form.Control as="textarea" rows={3} {...form.register('reason')} />
          </Form.Group>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="outline-secondary" onClick={onHide}>
            Keep
          </Button>
          <Button variant="danger" type="submit" disabled={mutation.isPending}>
            Mark lost
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  )
}
