import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Form, Modal } from 'react-bootstrap'
import { useForm } from 'react-hook-form'
import { Link, useParams } from 'react-router-dom'
import { applyApiFieldErrors } from '../../api/client'
import { fetchClients } from '../../api/clients'
import { fetchInsurers } from '../../api/insurers'
import { fetchPolicy, updatePolicy } from '../../api/policies'
import { fetchUsers } from '../../api/users'
import { PriorityChip, StatusChip } from '../../components/display/StatusChips'
import { EmptyState, ErrorBanner, LoadingBlock } from '../../components/feedback/PageFeedback'
import { useToast } from '../../components/feedback/ToastProvider'
import { daysRemainingLabel, formatDateIn, formatDateTimeIst, humanizeEnum, urgencyFromDays } from '../../lib/format'
import { formatInr } from '../../lib/money'
import { defaultPolicyFormValues, PolicyFormFields, type PolicyFormValues } from './PolicyFormFields'
import type { PolicyDetails } from '../../types/api'
import { stageLabel } from '../renewals/renewalDisplay'

function toFormValues(policy: PolicyDetails): PolicyFormValues {
  return {
    policyNumber: policy.policyNumber,
    clientPublicId: policy.clientPublicId,
    insurerPublicId: policy.insurerPublicId,
    policyType: policy.policyType,
    premium: policy.premium,
    sumInsured: policy.sumInsured,
    commissionPercentage: policy.commissionPercentage,
    startDate: policy.startDate,
    expiryDate: policy.expiryDate,
    assignedUserPublicId: policy.assignedUserPublicId ?? '',
    notes: policy.notes ?? '',
  }
}

export function PolicyDetailPage() {
  const { publicId = '' } = useParams()
  const queryClient = useQueryClient()
  const { showToast } = useToast()
  const [editing, setEditing] = useState(false)

  const policyQuery = useQuery({
    queryKey: ['policy', publicId],
    queryFn: () => fetchPolicy(publicId),
    enabled: Boolean(publicId),
  })

  const clientsQuery = useQuery({
    queryKey: ['clients', 'active-options'],
    queryFn: () => fetchClients({ isActive: 'true', pageSize: 100 }),
  })
  const insurersQuery = useQuery({ queryKey: ['insurers'], queryFn: fetchInsurers })
  const usersQuery = useQuery({ queryKey: ['users'], queryFn: fetchUsers })

  const policy = policyQuery.data
  const form = useForm<PolicyFormValues>({
    values: policy ? toFormValues(policy) : defaultPolicyFormValues(),
  })

  const updateMutation = useMutation({
    mutationFn: (values: PolicyFormValues) =>
      updatePolicy(publicId, {
        policyNumber: values.policyNumber.trim(),
        clientPublicId: values.clientPublicId,
        insurerPublicId: values.insurerPublicId,
        policyType: values.policyType,
        startDate: values.startDate,
        expiryDate: values.expiryDate,
        premium: Number(values.premium),
        sumInsured: Number(values.sumInsured),
        commissionPercentage: Number(values.commissionPercentage),
        assignedUserPublicId: values.assignedUserPublicId || undefined,
        notes: values.notes.trim() || undefined,
      }),
    onSuccess: (updated) => {
      showToast('Policy updated', `Commission ${formatInr(updated.commissionAmount)}.`, 'success')
      setEditing(false)
      void queryClient.invalidateQueries({ queryKey: ['policy', publicId] })
      void queryClient.invalidateQueries({ queryKey: ['policies'] })
      void queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    },
    onError: (error: Error) => {
      applyApiFieldErrors(error, form.setError)
      showToast('Could not update policy', error.message, 'danger')
    },
  })

  if (policyQuery.isError) {
    return (
      <div>
        <div className="page-heading">
          <Link to="/policies" className="text-decoration-none">
            ← Policies
          </Link>
          <h2 className="mt-2">Policy not found</h2>
        </div>
        <ErrorBanner>This policy is not in your book, or the API could not be reached.</ErrorBanner>
      </div>
    )
  }

  if (!policy) {
    return (
      <div>
        <div className="page-heading">
          <h2>Policy</h2>
        </div>
        <LoadingBlock label="Loading policy…" />
      </div>
    )
  }

  const urgency = urgencyFromDays(policy.daysRemaining)

  return (
    <div>
      <div className="page-heading d-flex justify-content-between align-items-start gap-3">
        <div>
          <Link to="/policies" className="text-decoration-none">
            ← Policies
          </Link>
          <h2 className="mt-2 mb-1">{policy.policyNumber}</h2>
          <p className="mb-0">
            {policy.clientName} · {humanizeEnum(policy.policyType)} · <StatusChip status={policy.status} />
          </p>
        </div>
        <Button className="btn-gold" onClick={() => setEditing(true)}>
          Edit
        </Button>
      </div>

      <section className={`content-card expiry-hero expiry-hero-${urgency} mb-4`}>
        <div className="section-kicker">Policy expiry</div>
        <div className="expiry-hero-date">{formatDateIn(policy.expiryDate)}</div>
        <div className={`expiry-hero-days${policy.daysRemaining <= 0 ? ' is-due-now' : ''}`}>
          {daysRemainingLabel(policy.daysRemaining)}
        </div>
        <div className="expiry-hero-meta">
          <span className="num">{formatInr(policy.premium)}</span>
          {policy.renewalPriority && <PriorityChip priority={policy.renewalPriority} />}
          {policy.renewalPublicId ? (
            <Link to={`/renewals/${policy.renewalPublicId}`}>
              Open renewal · {policy.renewalStatus ? humanizeEnum(policy.renewalStatus) : 'View'}
            </Link>
          ) : (
            <span>No open renewal</span>
          )}
        </div>
      </section>

      <div className="row g-4">
        <div className="col-lg-7">
          <section className="content-card">
            <h3 className="h6 text-uppercase text-muted">Policy</h3>
            <dl className="detail-list mb-0">
              <div>
                <dt>Client</dt>
                <dd>
                  <Link to={`/clients/${policy.clientPublicId}`}>{policy.clientName}</Link>
                </dd>
              </div>
              <div>
                <dt>Policy number</dt>
                <dd>{policy.policyNumber}</dd>
              </div>
              <div>
                <dt>Insurer</dt>
                <dd>{policy.insurerName}</dd>
              </div>
              <div>
                <dt>Policy type</dt>
                <dd>{humanizeEnum(policy.policyType)}</dd>
              </div>
              <div>
                <dt>Premium</dt>
                <dd>{formatInr(policy.premium)}</dd>
              </div>
              <div>
                <dt>Sum insured</dt>
                <dd>{formatInr(policy.sumInsured)}</dd>
              </div>
              <div>
                <dt>Commission %</dt>
                <dd>{policy.commissionPercentage.toFixed(2)}%</dd>
              </div>
              <div>
                <dt>Commission amount</dt>
                <dd>{formatInr(policy.commissionAmount)}</dd>
              </div>
              <div>
                <dt>Start</dt>
                <dd>{formatDateIn(policy.startDate)}</dd>
              </div>
              <div>
                <dt>Expiry</dt>
                <dd>
                  {formatDateIn(policy.expiryDate)} · {daysRemainingLabel(policy.daysRemaining)}
                </dd>
              </div>
              <div>
                <dt>Renewal</dt>
                <dd>
                  {policy.renewalPublicId ? (
                    <Link to={`/renewals/${policy.renewalPublicId}`}>
                      {humanizeEnum(policy.renewalStatus)}
                      {policy.renewalStage ? ` · ${stageLabel(policy.renewalStage)}` : ''}
                    </Link>
                  ) : (
                    '—'
                  )}
                </dd>
              </div>
              <div>
                <dt>Assigned employee</dt>
                <dd>{policy.assignedUserName ?? 'Unassigned'}</dd>
              </div>
              {policy.previousPolicyPublicId && (
                <div>
                  <dt>Previous term</dt>
                  <dd>
                    <Link to={`/policies/${policy.previousPolicyPublicId}`}>View previous policy</Link>
                  </dd>
                </div>
              )}
              {policy.nextPolicyPublicId && (
                <div>
                  <dt>Next term</dt>
                  <dd>
                    <Link to={`/policies/${policy.nextPolicyPublicId}`}>View next policy</Link>
                  </dd>
                </div>
              )}
              {policy.notes && (
                <div>
                  <dt>Notes</dt>
                  <dd>{policy.notes}</dd>
                </div>
              )}
            </dl>
          </section>
        </div>
        <div className="col-lg-5">
          <section className="content-card">
            <h3 className="h6 text-uppercase text-muted">Activity timeline</h3>
            {policy.activities.length === 0 && (
              <EmptyState icon="bi-clock-history" title="No activity recorded yet" description="Edits and renewal events for this policy will appear here." />
            )}
            {policy.activities.length > 0 && (
              <ol className="activity-timeline">
                {policy.activities.map((activity) => (
                  <li key={activity.publicId}>
                    <strong>{humanizeEnum(activity.activityType)}</strong>
                    <div>{activity.description}</div>
                    <div className="text-muted small">
                      {formatDateTimeIst(activity.createdAtUtc)}
                      {activity.userName ? ` · ${activity.userName}` : ''}
                    </div>
                  </li>
                ))}
              </ol>
            )}
          </section>
        </div>
      </div>

      <Modal show={editing} onHide={() => setEditing(false)} size="lg" centered>
        <Form onSubmit={form.handleSubmit((values) => updateMutation.mutate(values))}>
          <Modal.Header closeButton>
            <Modal.Title>Edit policy</Modal.Title>
          </Modal.Header>
          <Modal.Body>
            <PolicyFormFields
              register={form.register}
              control={form.control}
              errors={form.formState.errors}
              clients={clientsQuery.data?.items ?? []}
              insurers={insurersQuery.data ?? []}
              users={usersQuery.data ?? []}
            />
          </Modal.Body>
          <Modal.Footer>
            <Button variant="outline-secondary" onClick={() => setEditing(false)}>
              Cancel
            </Button>
            <Button className="btn-gold" type="submit" disabled={updateMutation.isPending}>
              Save changes
            </Button>
          </Modal.Footer>
        </Form>
      </Modal>
    </div>
  )
}
