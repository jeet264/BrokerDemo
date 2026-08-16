import { useEffect, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Form, Modal } from 'react-bootstrap'
import { useForm } from 'react-hook-form'
import { Link, useSearchParams } from 'react-router-dom'
import { applyApiFieldErrors } from '../../api/client'
import { fetchClients } from '../../api/clients'
import { fetchInsurers } from '../../api/insurers'
import { createPolicy, fetchPolicies } from '../../api/policies'
import { fetchUsers } from '../../api/users'
import { useToast } from '../../components/feedback/ToastProvider'
import { daysRemainingShort, formatDateIn, humanizeEnum } from '../../lib/format'
import { formatInr } from '../../lib/money'
import { StatusChip } from '../../components/display/StatusChips'
import { defaultPolicyFormValues, POLICY_TYPES, PolicyFormFields, type PolicyFormValues } from './PolicyFormFields'

function toRequest(values: PolicyFormValues) {
  return {
    policyNumber: values.policyNumber.trim() || undefined,
    clientPublicId: values.clientPublicId,
    insurerPublicId: values.insurerPublicId,
    policyType: values.policyType,
    startDate: values.startDate,
    expiryDate: values.expiryDate,
    premium: Number(values.premium),
    sumInsured: Number(values.sumInsured),
    commissionPercentage: Number(values.commissionPercentage),
    assignedUserPublicId: values.assignedUserPublicId || undefined,
    vehicleNumber: values.vehicleNumber.trim() || undefined,
    notes: values.notes.trim() || undefined,
  }
}

export function PoliciesPage() {
  const queryClient = useQueryClient()
  const { showToast } = useToast()
  const [searchParams] = useSearchParams()
  const clientPublicId = searchParams.get('client') ?? ''
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('Active')
  const [policyType, setPolicyType] = useState('')
  const [insurerPublicId, setInsurerPublicId] = useState('')
  const [assignedUserPublicId, setAssignedUserPublicId] = useState('')
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')
  const [showAdd, setShowAdd] = useState(false)

  useEffect(() => {
    const timer = window.setTimeout(() => setSearch(searchInput.trim()), 300)
    return () => window.clearTimeout(timer)
  }, [searchInput])

  const listQuery = useQuery({
    queryKey: ['policies', search, status, policyType, insurerPublicId, assignedUserPublicId, fromDate, toDate, clientPublicId],
    queryFn: () =>
      fetchPolicies({
        search: search || undefined,
        status: status || undefined,
        policyType: policyType || undefined,
        insurerPublicId: insurerPublicId || undefined,
        assignedUserPublicId: assignedUserPublicId || undefined,
        clientPublicId: clientPublicId || undefined,
        fromDate: fromDate || undefined,
        toDate: toDate || undefined,
        pageSize: 50,
      }),
  })

  const clientsQuery = useQuery({
    queryKey: ['clients', 'active-options'],
    queryFn: () => fetchClients({ isActive: 'true', pageSize: 100 }),
  })
  const insurersQuery = useQuery({ queryKey: ['insurers'], queryFn: fetchInsurers })
  const usersQuery = useQuery({ queryKey: ['users'], queryFn: fetchUsers })

  const form = useForm<PolicyFormValues>({ defaultValues: defaultPolicyFormValues() })

  const createMutation = useMutation({
    mutationFn: (values: PolicyFormValues) => createPolicy(toRequest(values)),
    onSuccess: (policy) => {
      showToast('Policy created', `${policy.policyNumber} commission ${formatInr(policy.commissionAmount)}.`, 'success')
      setShowAdd(false)
      form.reset(defaultPolicyFormValues())
      void queryClient.invalidateQueries({ queryKey: ['policies'] })
      void queryClient.invalidateQueries({ queryKey: ['clients'] })
      void queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    },
    onError: (error: Error) => {
      applyApiFieldErrors(error, form.setError)
      showToast('Could not create policy', error.message, 'danger')
    },
  })

  const policies = listQuery.data?.items ?? []
  const clients = clientsQuery.data?.items ?? []
  const insurers = insurersQuery.data ?? []
  const users = usersQuery.data ?? []

  return (
    <div>
      <div className="page-heading d-flex justify-content-between align-items-start gap-3">
        <div>
          <h2>Policies</h2>
          <p>Current-term policies by default. After a renewal, the new expiry is shown — never the expired one.</p>
        </div>
        <div className="d-flex gap-2">
          <Link className="btn btn-outline-secondary" to="/policies/import">
            Import from Excel/CSV
          </Link>
          <Button className="btn-gold" onClick={() => setShowAdd(true)}>
            + Add Policy
          </Button>
        </div>
      </div>

      <section className="content-card mb-3">
        <div className="filter-bar filter-bar-policies">
          <div className="filter-field">
            <label htmlFor="policy-search">Search</label>
            <Form.Control
              id="policy-search"
              placeholder="Search policy, client, or insurer"
              value={searchInput}
              onChange={(event) => setSearchInput(event.target.value)}
            />
          </div>
          <div className="filter-field">
            <label htmlFor="expiry-from">Expiry from</label>
            <Form.Control
              id="expiry-from"
              type="date"
              value={fromDate}
              onChange={(event) => setFromDate(event.target.value)}
            />
          </div>
          <div className="filter-field">
            <label htmlFor="expiry-to">Expiry to</label>
            <Form.Control
              id="expiry-to"
              type="date"
              value={toDate}
              onChange={(event) => setToDate(event.target.value)}
            />
          </div>
          <Form.Select value={insurerPublicId} onChange={(event) => setInsurerPublicId(event.target.value)} aria-label="Filter by insurer">
            <option value="">All insurers</option>
            {insurers.map((insurer) => (
              <option key={insurer.publicId} value={insurer.publicId}>
                {insurer.name}
              </option>
            ))}
          </Form.Select>
          <Form.Select value={policyType} onChange={(event) => setPolicyType(event.target.value)}>
            <option value="">All types</option>
            {POLICY_TYPES.map((type) => (
              <option key={type} value={type}>
                {humanizeEnum(type)}
              </option>
            ))}
          </Form.Select>
          <Form.Select value={status} onChange={(event) => setStatus(event.target.value)}>
            <option value="">All statuses</option>
            <option value="Active">Active</option>
            <option value="PendingRenewal">Pending renewal</option>
            <option value="Expired">Expired</option>
            <option value="Cancelled">Cancelled</option>
          </Form.Select>
          <Form.Select value={assignedUserPublicId} onChange={(event) => setAssignedUserPublicId(event.target.value)}>
            <option value="">All employees</option>
            {users.map((user) => (
              <option key={user.publicId} value={user.publicId}>
                {user.fullName}
              </option>
            ))}
          </Form.Select>
        </div>
      </section>

      <section className="content-card">
        {listQuery.isError && <div className="alert alert-danger">Could not load policies. Check your connection and try again.</div>}
        {listQuery.isLoading && (
          <div className="loading-block" role="status">
            <span className="spinner-border spinner-border-sm" aria-hidden />
            <span>Loading policies…</span>
          </div>
        )}
        {!listQuery.isLoading && policies.length === 0 && (
          <div className="empty-state">
            <i className="bi bi-file-earmark-text" aria-hidden />
            <h3>No policies match these filters</h3>
            <p>Adjust search or dates, or add a policy to the book.</p>
          </div>
        )}
        {policies.length > 0 && (
          <div className="table-responsive table-scroll">
            <table className="table align-middle mb-0">
              <thead>
                <tr>
                  <th>Policy number</th>
                  <th>Client</th>
                  <th>Policy type</th>
                  <th>Insurer</th>
                  <th className="num">Premium</th>
                  <th>Start date</th>
                  <th>Expiry date</th>
                  <th>Days remaining</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {policies.map((policy) => (
                  <tr key={policy.publicId} className={policy.daysRemaining < 0 ? 'row-attention' : undefined}>
                    <td>
                      <strong>{policy.policyNumber}</strong>
                    </td>
                    <td>{policy.clientName}</td>
                    <td>{humanizeEnum(policy.policyType)}</td>
                    <td>{policy.insurerName}</td>
                    <td className="num">{formatInr(policy.premium)}</td>
                    <td>{formatDateIn(policy.startDate)}</td>
                    <td>{formatDateIn(policy.expiryDate)}</td>
                    <td className={policy.daysRemaining <= 0 ? 'is-due-now' : undefined}>{daysRemainingShort(policy.daysRemaining)}</td>
                    <td>
                      <StatusChip status={policy.status} />
                    </td>
                    <td>
                      <div className="table-actions">
                        <Link to={`/policies/${policy.publicId}`} className="btn btn-sm btn-outline-secondary">
                          View
                        </Link>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <Modal
        show={showAdd}
        onHide={() => {
          setShowAdd(false)
          form.reset(defaultPolicyFormValues())
        }}
        size="lg"
        centered
      >
        <Form onSubmit={form.handleSubmit((values) => createMutation.mutate(values))}>
          <Modal.Header closeButton>
            <Modal.Title>Add Policy</Modal.Title>
          </Modal.Header>
          <Modal.Body>
            <PolicyFormFields
              register={form.register}
              control={form.control}
              errors={form.formState.errors}
              clients={clients}
              insurers={insurers}
              users={users}
              policyNumberOptional
            />
          </Modal.Body>
          <Modal.Footer>
            <Button
              variant="outline-secondary"
              onClick={() => {
                setShowAdd(false)
                form.reset(defaultPolicyFormValues())
              }}
            >
              Cancel
            </Button>
            <Button className="btn-gold" type="submit" disabled={createMutation.isPending}>
              Save policy
            </Button>
          </Modal.Footer>
        </Form>
      </Modal>
    </div>
  )
}
