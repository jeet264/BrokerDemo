import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Form, Modal } from 'react-bootstrap'
import { useForm } from 'react-hook-form'
import { Link, useParams } from 'react-router-dom'
import { applyApiFieldErrors } from '../../api/client'
import { fetchClients } from '../../api/clients'
import { fetchPolicies } from '../../api/policies'
import { fetchRenewals } from '../../api/renewals'
import { cancelTask, completeTask, fetchTask, reassignTask, updateTask } from '../../api/tasks'
import { fetchUsers } from '../../api/users'
import { useToast } from '../../components/feedback/ToastProvider'
import type { WorkTaskDetails } from '../../types/api'
import {
  datetimeLocalToUtc,
  formatIst,
  isOpenTask,
  priorityClass,
  TASK_PRIORITIES,
  toDatetimeLocal,
} from './taskDisplay'

type ActionModal = 'complete' | 'reassign' | 'edit' | 'cancel' | null

interface EditForm {
  title: string
  description: string
  dueDateLocal: string
  priority: string
  clientPublicId: string
  policyPublicId: string
  renewalPublicId: string
}

interface ReassignForm {
  assignedUserPublicId: string
}

export function TaskDetailPage() {
  const { publicId = '' } = useParams()
  const queryClient = useQueryClient()
  const { showToast } = useToast()
  const [action, setAction] = useState<ActionModal>(null)

  const taskQuery = useQuery({
    queryKey: ['task', publicId],
    queryFn: () => fetchTask(publicId),
    enabled: Boolean(publicId),
  })

  const task = taskQuery.data
  const open = task ? isOpenTask(task.status) : false

  const refreshFrom = (updated: WorkTaskDetails) => {
    queryClient.setQueryData(['task', publicId], updated)
    void queryClient.invalidateQueries({ queryKey: ['tasks'] })
    void queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    void queryClient.invalidateQueries({ queryKey: ['renewals'] })
  }

  if (taskQuery.isError) {
    return (
      <div>
        <div className="page-heading">
          <Link to="/tasks" className="text-decoration-none">
            ← Tasks
          </Link>
          <h2 className="mt-2">Task not found</h2>
        </div>
        <div className="alert alert-danger">This task is not in your book, or the API could not be reached.</div>
      </div>
    )
  }

  if (!task) {
    return (
      <div>
        <div className="page-heading">
          <h2>Task</h2>
          <p className="text-muted">Loading task…</p>
        </div>
      </div>
    )
  }

  return (
    <div>
      <div className="page-heading d-flex justify-content-between align-items-start gap-3">
        <div>
          <Link to="/tasks" className="text-decoration-none">
            ← Tasks
          </Link>
          <h2 className="mt-2 mb-1">{task.title}</h2>
          <p className="mb-0">
            <span className={priorityClass(task.priority)}>{task.priority}</span>
            <span className="ms-2">{task.status}</span>
          </p>
        </div>
      </div>

      <div className="row g-4">
        <div className="col-lg-8">
          <section className="content-card">
            <h3 className="h6 text-uppercase text-muted">Task</h3>
            <dl className="detail-list mb-0">
              <div>
                <dt>Title</dt>
                <dd>{task.title}</dd>
              </div>
              <div>
                <dt>Description</dt>
                <dd>{task.description ?? '—'}</dd>
              </div>
              <div>
                <dt>Related client</dt>
                <dd>
                  {task.clientPublicId ? (
                    <Link to={`/clients/${task.clientPublicId}`}>{task.clientName}</Link>
                  ) : (
                    '—'
                  )}
                </dd>
              </div>
              <div>
                <dt>Related policy</dt>
                <dd>
                  {task.policyPublicId ? (
                    <Link to={`/policies/${task.policyPublicId}`}>{task.policyNumber}</Link>
                  ) : (
                    '—'
                  )}
                </dd>
              </div>
              <div>
                <dt>Related renewal</dt>
                <dd>
                  {task.renewalPublicId ? (
                    <Link to={`/renewals/${task.renewalPublicId}`}>
                      {task.renewalPolicyNumber ?? 'Open renewal'}
                    </Link>
                  ) : (
                    '—'
                  )}
                </dd>
              </div>
              <div>
                <dt>Assigned user</dt>
                <dd>{task.assignedUserName ?? 'Unassigned'}</dd>
              </div>
              <div>
                <dt>Due date</dt>
                <dd className={task.status === 'Overdue' ? 'is-due-now' : undefined}>{formatIst(task.dueDateUtc)}</dd>
              </div>
              <div>
                <dt>Priority</dt>
                <dd>
                  <span className={priorityClass(task.priority)}>{task.priority}</span>
                </dd>
              </div>
              <div>
                <dt>Status</dt>
                <dd>{task.status}</dd>
              </div>
              {task.completedAtUtc && (
                <div>
                  <dt>Completed</dt>
                  <dd>{formatIst(task.completedAtUtc)}</dd>
                </div>
              )}
            </dl>
          </section>
        </div>
        <div className="col-lg-4">
          <section className="content-card">
            <h3 className="h6 text-uppercase text-muted">Actions</h3>
            {open ? (
              <div className="renewal-actions">
                <Button className="btn-gold" onClick={() => setAction('complete')}>
                  Complete
                </Button>
                <Button variant="outline-secondary" onClick={() => setAction('reassign')}>
                  Reassign
                </Button>
                <Button variant="outline-secondary" onClick={() => setAction('edit')}>
                  Edit
                </Button>
                <Button variant="outline-danger" onClick={() => setAction('cancel')}>
                  Cancel
                </Button>
              </div>
            ) : (
              <p className="text-muted mb-0">This task is {task.status.toLowerCase()} and can no longer be changed.</p>
            )}
          </section>
        </div>
      </div>

      <CompleteModal
        show={action === 'complete'}
        title={task.title}
        publicId={publicId}
        onHide={() => setAction(null)}
        onSaved={(updated) => {
          refreshFrom(updated)
          setAction(null)
          showToast('Task completed', `${updated.title} was stamped complete.`, 'success')
        }}
      />
      <ReassignModal
        show={action === 'reassign'}
        publicId={publicId}
        assignedUserPublicId={task.assignedUserPublicId ?? ''}
        onHide={() => setAction(null)}
        onSaved={(updated) => {
          refreshFrom(updated)
          setAction(null)
          showToast('Task reassigned', updated.assignedUserName ?? 'Assigned.', 'success')
        }}
      />
      <EditModal
        show={action === 'edit'}
        publicId={publicId}
        task={task}
        onHide={() => setAction(null)}
        onSaved={(updated) => {
          refreshFrom(updated)
          setAction(null)
          showToast('Task updated', updated.title, 'success')
        }}
      />
      <CancelModal
        show={action === 'cancel'}
        title={task.title}
        publicId={publicId}
        onHide={() => setAction(null)}
        onSaved={(updated) => {
          refreshFrom(updated)
          setAction(null)
          showToast('Task cancelled', updated.title, 'info')
        }}
      />
    </div>
  )
}

function CompleteModal({
  show,
  title,
  publicId,
  onHide,
  onSaved,
}: {
  show: boolean
  title: string
  publicId: string
  onHide: () => void
  onSaved: (updated: WorkTaskDetails) => void
}) {
  const { showToast } = useToast()
  const mutation = useMutation({
    mutationFn: () => completeTask(publicId),
    onSuccess: onSaved,
    onError: (error: Error) => showToast('Could not complete task', error.message, 'danger'),
  })

  return (
    <Modal show={show} onHide={onHide} centered>
      <Modal.Header closeButton>
        <Modal.Title>Complete task</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        Mark <strong>{title}</strong> complete? Completed time is stored in UTC and a TaskCompleted activity is written.
      </Modal.Body>
      <Modal.Footer>
        <Button variant="outline-secondary" onClick={onHide}>
          Keep open
        </Button>
        <Button className="btn-gold" onClick={() => mutation.mutate()} disabled={mutation.isPending}>
          Complete
        </Button>
      </Modal.Footer>
    </Modal>
  )
}

function ReassignModal({
  show,
  publicId,
  assignedUserPublicId,
  onHide,
  onSaved,
}: {
  show: boolean
  publicId: string
  assignedUserPublicId: string
  onHide: () => void
  onSaved: (updated: WorkTaskDetails) => void
}) {
  const { showToast } = useToast()
  const usersQuery = useQuery({ queryKey: ['users'], queryFn: fetchUsers })
  const form = useForm<ReassignForm>({ values: { assignedUserPublicId } })
  const mutation = useMutation({
    mutationFn: (values: ReassignForm) => reassignTask(publicId, values.assignedUserPublicId),
    onSuccess: onSaved,
    onError: (error: Error) => {
      applyApiFieldErrors(error, form.setError)
      showToast('Could not reassign', error.message, 'danger')
    },
  })

  return (
    <Modal show={show} onHide={onHide} centered>
      <Form onSubmit={form.handleSubmit((values) => mutation.mutate(values))}>
        <Modal.Header closeButton>
          <Modal.Title>Reassign task</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form.Group>
            <Form.Label>Assigned user</Form.Label>
            <Form.Select
              isInvalid={Boolean(form.formState.errors.assignedUserPublicId)}
              {...form.register('assignedUserPublicId', { required: 'Assigned user is required' })}
            >
              <option value="">Select employee</option>
              {(usersQuery.data ?? []).map((user) => (
                <option key={user.publicId} value={user.publicId}>
                  {user.fullName}
                </option>
              ))}
            </Form.Select>
            <Form.Control.Feedback type="invalid">
              {form.formState.errors.assignedUserPublicId?.message}
            </Form.Control.Feedback>
          </Form.Group>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="outline-secondary" onClick={onHide}>
            Cancel
          </Button>
          <Button className="btn-gold" type="submit" disabled={mutation.isPending}>
            Reassign
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  )
}

function EditModal({
  show,
  publicId,
  task,
  onHide,
  onSaved,
}: {
  show: boolean
  publicId: string
  task: WorkTaskDetails
  onHide: () => void
  onSaved: (updated: WorkTaskDetails) => void
}) {
  const { showToast } = useToast()
  const clientsQuery = useQuery({
    queryKey: ['clients', 'active-options'],
    queryFn: () => fetchClients({ isActive: 'true', pageSize: 100 }),
  })
  const policiesQuery = useQuery({
    queryKey: ['policies', 'task-options'],
    queryFn: () => fetchPolicies({ pageSize: 100 }),
  })
  const renewalsQuery = useQuery({
    queryKey: ['renewals', 'all'],
    queryFn: () => fetchRenewals({ dueFilter: 'all', pageSize: 50 }),
  })
  const form = useForm<EditForm>({
    values: {
      title: task.title,
      description: task.description ?? '',
      dueDateLocal: toDatetimeLocal(task.dueDateUtc),
      priority: task.priority,
      clientPublicId: task.clientPublicId ?? '',
      policyPublicId: task.policyPublicId ?? '',
      renewalPublicId: task.renewalPublicId ?? '',
    },
  })
  const mutation = useMutation({
    mutationFn: (values: EditForm) =>
      updateTask(publicId, {
        title: values.title.trim(),
        description: values.description.trim() || undefined,
        dueDateUtc: datetimeLocalToUtc(values.dueDateLocal),
        priority: values.priority,
        clientPublicId: values.clientPublicId || undefined,
        policyPublicId: values.policyPublicId || undefined,
        renewalPublicId: values.renewalPublicId || undefined,
      }),
    onSuccess: onSaved,
    onError: (error: Error) => {
      applyApiFieldErrors(error, form.setError)
      showToast('Could not update task', error.message, 'danger')
    },
  })

  const clients = clientsQuery.data?.items ?? []
  const policies = policiesQuery.data?.items ?? []
  const renewals = renewalsQuery.data?.items ?? []

  return (
    <Modal show={show} onHide={onHide} centered size="lg">
      <Form onSubmit={form.handleSubmit((values) => mutation.mutate(values))}>
        <Modal.Header closeButton>
          <Modal.Title>Edit task</Modal.Title>
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
          <div className="row">
            <div className="col-md-6">
              <Form.Group className="mb-3">
                <Form.Label>Due date</Form.Label>
                <Form.Control
                  type="datetime-local"
                  isInvalid={Boolean(form.formState.errors.dueDateLocal)}
                  {...form.register('dueDateLocal', { required: 'Due date is required' })}
                />
              </Form.Group>
            </div>
            <div className="col-md-6">
              <Form.Group className="mb-3">
                <Form.Label>Priority</Form.Label>
                <Form.Select {...form.register('priority', { required: true })}>
                  {TASK_PRIORITIES.map((item) => (
                    <option key={item} value={item}>
                      {item}
                    </option>
                  ))}
                </Form.Select>
              </Form.Group>
            </div>
          </div>
          <Form.Group className="mb-3">
            <Form.Label>Related client</Form.Label>
            <Form.Select {...form.register('clientPublicId')}>
              <option value="">None</option>
              {clients.map((client) => (
                <option key={client.publicId} value={client.publicId}>
                  {client.companyName}
                </option>
              ))}
            </Form.Select>
          </Form.Group>
          <Form.Group className="mb-3">
            <Form.Label>Related policy</Form.Label>
            <Form.Select {...form.register('policyPublicId')}>
              <option value="">None</option>
              {policies.map((policy) => (
                <option key={policy.publicId} value={policy.publicId}>
                  {policy.policyNumber} · {policy.clientName}
                </option>
              ))}
            </Form.Select>
          </Form.Group>
          <Form.Group>
            <Form.Label>Related renewal</Form.Label>
            <Form.Select {...form.register('renewalPublicId')}>
              <option value="">None</option>
              {renewals.map((renewal) => (
                <option key={renewal.publicId} value={renewal.publicId}>
                  {renewal.policyNumber} · {renewal.clientName}
                </option>
              ))}
            </Form.Select>
          </Form.Group>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="outline-secondary" onClick={onHide}>
            Cancel
          </Button>
          <Button className="btn-gold" type="submit" disabled={mutation.isPending}>
            Save changes
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  )
}

function CancelModal({
  show,
  title,
  publicId,
  onHide,
  onSaved,
}: {
  show: boolean
  title: string
  publicId: string
  onHide: () => void
  onSaved: (updated: WorkTaskDetails) => void
}) {
  const { showToast } = useToast()
  const mutation = useMutation({
    mutationFn: () => cancelTask(publicId),
    onSuccess: onSaved,
    onError: (error: Error) => showToast('Could not cancel task', error.message, 'danger'),
  })

  return (
    <Modal show={show} onHide={onHide} centered>
      <Modal.Header closeButton>
        <Modal.Title>Cancel task</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        Cancel <strong>{title}</strong>? It will leave the open queue and will not be marked complete.
      </Modal.Body>
      <Modal.Footer>
        <Button variant="outline-secondary" onClick={onHide}>
          Keep
        </Button>
        <Button variant="danger" onClick={() => mutation.mutate()} disabled={mutation.isPending}>
          Cancel task
        </Button>
      </Modal.Footer>
    </Modal>
  )
}
