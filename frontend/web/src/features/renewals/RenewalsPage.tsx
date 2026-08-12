import { useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Form, Modal } from 'react-bootstrap'
import { useForm } from 'react-hook-form'
import { completeRenewal, fetchRenewals, markRenewalLost } from '../../api/renewals'
import { useToast } from '../../components/feedback/ToastProvider'
import type { RenewalListItem } from '../../types/api'

interface RenewForm {
  newExpiryDate: string
  premium: number
}

function addYears(isoDate: string, years: number) {
  const [year, month, day] = isoDate.split('-').map(Number)
  return `${year + years}-${String(month).padStart(2, '0')}-${String(day).padStart(2, '0')}`
}

function formatInr(amount: number) {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    maximumFractionDigits: 0,
  }).format(amount)
}

export function RenewalsPage() {
  const queryClient = useQueryClient()
  const { showToast } = useToast()
  const [statusFilter, setStatusFilter] = useState('Upcoming')
  const [selected, setSelected] = useState<RenewalListItem | null>(null)
  const [lostTarget, setLostTarget] = useState<RenewalListItem | null>(null)

  const listQuery = useQuery({
    queryKey: ['renewals', statusFilter],
    queryFn: () => fetchRenewals({ status: statusFilter, pageSize: 50 }),
  })

  const defaults = useMemo<RenewForm>(() => {
    if (!selected) {
      return { newExpiryDate: '', premium: 0 }
    }
    return {
      newExpiryDate: addYears(selected.expiryDate, 1),
      premium: selected.premium,
    }
  }, [selected])

  const form = useForm<RenewForm>({ values: defaults })

  const completeMutation = useMutation({
    mutationFn: (values: RenewForm) => completeRenewal(selected!.publicId, values),
    onSuccess: (result) => {
      showToast('Policy renewed', `Next term ${result.nextPolicyNumber ?? result.policyNumber} expires ${result.nextPolicyExpiryDate ?? result.expiryDate}.`, 'success')
      setSelected(null)
      void queryClient.invalidateQueries({ queryKey: ['renewals'] })
      void queryClient.invalidateQueries({ queryKey: ['policies'] })
      void queryClient.invalidateQueries({ queryKey: ['renewal-dashboard'] })
    },
    onError: (error: Error) => showToast('Could not renew', error.message, 'danger'),
  })

  const lostMutation = useMutation({
    mutationFn: () => markRenewalLost(lostTarget!.publicId),
    onSuccess: () => {
      showToast('Marked lost', 'The policy was cancelled. No new term was created.', 'info')
      setLostTarget(null)
      void queryClient.invalidateQueries({ queryKey: ['renewals'] })
      void queryClient.invalidateQueries({ queryKey: ['policies'] })
      void queryClient.invalidateQueries({ queryKey: ['renewal-dashboard'] })
    },
    onError: (error: Error) => showToast('Could not mark lost', error.message, 'danger'),
  })

  const renewals = listQuery.data?.items ?? []
  const openStatuses = ['Upcoming', 'InProgress', 'QuotationPending', 'ClientDecisionPending', 'Overdue']

  return (
    <div>
      <div className="page-heading d-flex justify-content-between align-items-start gap-3">
        <div>
          <h2>Renewals</h2>
          <p>Mark Renewed rolls the policy into a new term. Lists show the current expiry, not the expired one.</p>
        </div>
        <Form.Select value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)} style={{ maxWidth: 220 }}>
          <option value="Upcoming">Upcoming</option>
          <option value="Overdue">Overdue</option>
          <option value="InProgress">In progress</option>
          <option value="Renewed">Renewed</option>
          <option value="Lost">Lost</option>
        </Form.Select>
      </div>
      <section className="content-card">
        {listQuery.isError && <div className="alert alert-danger">Could not load renewals. Sign in and confirm the API is running.</div>}
        {listQuery.isLoading && <p className="text-muted mb-0">Loading renewals…</p>}
        {!listQuery.isLoading && renewals.length === 0 && <p className="text-muted mb-0">No renewals in this view.</p>}
        {renewals.length > 0 && (
          <div className="table-responsive">
            <table className="table align-middle mb-0">
              <thead>
                <tr>
                  <th>Policy</th>
                  <th>Client</th>
                  <th>Expiry</th>
                  <th>Days</th>
                  <th>Premium</th>
                  <th>Status</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {renewals.map((renewal) => (
                  <tr key={renewal.publicId}>
                    <td>
                      <strong>{renewal.policyNumber}</strong>
                      <div className="text-muted small">{renewal.policyType}</div>
                    </td>
                    <td>{renewal.clientName}</td>
                    <td>{renewal.expiryDate}</td>
                    <td>{renewal.daysRemaining}</td>
                    <td>{formatInr(renewal.premium)}</td>
                    <td>{renewal.status}</td>
                    <td className="text-end">
                      {openStatuses.includes(renewal.status) && (
                        <>
                          <Button size="sm" className="btn-gold me-2" onClick={() => setSelected(renewal)}>
                            Mark Renewed
                          </Button>
                          <Button size="sm" variant="outline-danger" onClick={() => setLostTarget(renewal)}>
                            Mark Lost
                          </Button>
                        </>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <Modal show={selected != null} onHide={() => setSelected(null)} centered>
        <Form onSubmit={form.handleSubmit((values) => completeMutation.mutate(values))}>
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
            <Button variant="outline-secondary" onClick={() => setSelected(null)}>
              Cancel
            </Button>
            <Button className="btn-gold" type="submit" disabled={completeMutation.isPending}>
              Confirm
            </Button>
          </Modal.Footer>
        </Form>
      </Modal>

      <Modal show={lostTarget != null} onHide={() => setLostTarget(null)} centered>
        <Modal.Header closeButton>
          <Modal.Title>Mark lost</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          Cancel {lostTarget?.policyNumber}? The policy will be marked Cancelled and no new term will be created.
        </Modal.Body>
        <Modal.Footer>
          <Button variant="outline-secondary" onClick={() => setLostTarget(null)}>
            Keep
          </Button>
          <Button variant="danger" onClick={() => lostMutation.mutate()} disabled={lostMutation.isPending}>
            Mark lost
          </Button>
        </Modal.Footer>
      </Modal>
    </div>
  )
}
