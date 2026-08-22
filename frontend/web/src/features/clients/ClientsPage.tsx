import { useEffect, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Form, Modal } from 'react-bootstrap'
import { useForm } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { applyApiFieldErrors } from '../../api/client'
import { createClient, fetchClients } from '../../api/clients'
import { fetchUsers } from '../../api/users'
import { ClientRowActions } from '../actions'
import { useToast } from '../../components/feedback/ToastProvider'
import { ClientFormFields, defaultClientFormValues, type ClientFormValues } from './ClientFormFields'
import { CustomSelect } from '../../components/display/CustomSelect'

export function ClientsPage() {
  const queryClient = useQueryClient()
  const { showToast } = useToast()
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState('')
  const [clientType, setClientType] = useState('')
  const [industry, setIndustry] = useState('')
  const [assignedUserPublicId, setAssignedUserPublicId] = useState('')
  const [status, setStatus] = useState('')
  const [showAdd, setShowAdd] = useState(false)

  useEffect(() => {
    const timer = window.setTimeout(() => setSearch(searchInput.trim()), 300)
    return () => window.clearTimeout(timer)
  }, [searchInput])

  const listQuery = useQuery({
    queryKey: ['clients', search, clientType, industry, assignedUserPublicId, status],
    queryFn: () =>
      fetchClients({
        search: search || undefined,
        clientType: clientType || undefined,
        industry: industry || undefined,
        assignedUserPublicId: assignedUserPublicId || undefined,
        isActive: status || undefined,
        pageSize: 50,
      }),
  })

  const usersQuery = useQuery({
    queryKey: ['users'],
    queryFn: fetchUsers,
  })

  const form = useForm<ClientFormValues>({ defaultValues: defaultClientFormValues })

  const createMutation = useMutation({
    mutationFn: (values: ClientFormValues) =>
      createClient({
        companyName: values.companyName.trim(),
        clientType: values.clientType,
        industry: values.industry.trim() || undefined,
        email: values.email.trim(),
        phone: values.phone.trim(),
        addressLine1: values.addressLine1.trim(),
        city: values.city.trim(),
        state: values.state.trim(),
        postalCode: values.postalCode.trim(),
        country: 'India',
        assignedUserPublicId: values.assignedUserPublicId || undefined,
        notes: values.notes.trim() || undefined,
      }),
    onSuccess: (client) => {
      showToast('Client added', `${client.companyName} is in the book.`, 'success')
      setShowAdd(false)
      form.reset(defaultClientFormValues)
      void queryClient.invalidateQueries({ queryKey: ['clients'] })
      void queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    },
    onError: (error: Error) => {
      applyApiFieldErrors(error, form.setError)
      showToast('Could not add client', error.message, 'danger')
    },
  })

  const clients = listQuery.data?.items ?? []
  const users = usersQuery.data ?? []

  return (
    <div>
      <div className="page-heading d-flex justify-content-between align-items-start gap-3">
        <div>
          <h2>Clients</h2>
          <p>The brokerage book — search, filter, and open a client to work the renewal.</p>
        </div>
        <div className="d-flex gap-2">
          <Link className="btn btn-outline-secondary" to="/clients/import">
            Import from Excel/CSV
          </Link>
          <Button className="btn-gold" onClick={() => setShowAdd(true)}>
            + Add Client
          </Button>
        </div>
      </div>

      <section className="content-card mb-3">
        <div className="filter-bar">
          <div className="filter-field">
            <label htmlFor="client-search">Search</label>
            <Form.Control
              id="client-search"
              placeholder="Search company, code, email, or phone"
              value={searchInput}
              onChange={(event) => setSearchInput(event.target.value)}
            />
          </div>
          <div className="filter-field">
            <label>Type</label>
            <CustomSelect
              value={clientType}
              onChange={setClientType}
              options={[
                { value: '', label: 'All types' },
                { value: 'Corporate', label: 'Corporate' },
                { value: 'SME', label: 'SME' },
                { value: 'Individual', label: 'Individual' },
              ]}
            />
          </div>
          <div className="filter-field">
            <label htmlFor="client-industry">Industry</label>
            <Form.Control
              id="client-industry"
              placeholder="Industry"
              value={industry}
              onChange={(event) => setIndustry(event.target.value)}
            />
          </div>
          <div className="filter-field">
            <label>Broker</label>
            <CustomSelect
              value={assignedUserPublicId}
              onChange={setAssignedUserPublicId}
              options={[
                { value: '', label: 'All brokers' },
                ...users.map((user) => ({ value: user.publicId, label: user.fullName })),
              ]}
            />
          </div>
          <div className="filter-field">
            <label>Status</label>
            <CustomSelect
              value={status}
              onChange={setStatus}
              options={[
                { value: '', label: 'All statuses' },
                { value: 'true', label: 'Active' },
                { value: 'false', label: 'Inactive' },
              ]}
            />
          </div>
          <div className="filter-field align-self-end">
            <button
              type="button"
              className="btn btn-sm btn-outline-danger w-100"
              style={{ height: '38px' }}
              disabled={!searchInput && !clientType && !industry && !assignedUserPublicId && !status}
              onClick={() => {
                setSearchInput('')
                setSearch('')
                setClientType('')
                setIndustry('')
                setAssignedUserPublicId('')
                setStatus('')
              }}
              title="Reset all filters"
            >
              <i className="bi bi-arrow-counterclockwise me-1" /> Reset
            </button>
          </div>
        </div>
      </section>

      <section className="content-card">
        {listQuery.isError && (
          <div className="alert alert-danger" role="alert">
            Could not load clients. Check your connection and try again.
          </div>
        )}
        {listQuery.isLoading && (
          <div className="loading-block" role="status">
            <span className="spinner-border spinner-border-sm" aria-hidden />
            <span>Loading clients…</span>
          </div>
        )}
        {!listQuery.isLoading && clients.length === 0 && (
          <div className="empty-state">
            <i className="bi bi-people" aria-hidden />
            <h3>No clients match these filters</h3>
            <p>Try a different search, or add a client to the book.</p>
          </div>
        )}
        {clients.length > 0 && (
          <div className="table-responsive table-scroll">
            <table className="table align-middle mb-0">
              <thead>
                <tr>
                  <th>Company</th>
                  <th>Type</th>
                  <th>Industry</th>
                  <th>Policies</th>
                  <th>Renewals</th>
                  <th>Assigned to</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {clients.map((client) => (
                  <tr key={client.publicId}>
                    <td>
                      <strong>{client.companyName}</strong>
                      <div className="text-muted small">{client.clientCode}</div>
                    </td>
                    <td>{client.clientType}</td>
                    <td>{client.industry ?? '—'}</td>
                    <td className="num">{client.policyCount}</td>
                    <td className="num">{client.renewalCount}</td>
                    <td>{client.assignedUserName ?? '—'}</td>
                    <td>
                      <span className={client.isActive ? 'status-pill' : 'priority-chip priority-chip-low'}>
                        {client.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td>
                      <ClientRowActions
                        publicId={client.publicId}
                        companyName={client.companyName}
                        phone={client.phone}
                      />
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
          form.reset(defaultClientFormValues)
        }}
        size="lg"
        centered
      >
        <Form onSubmit={form.handleSubmit((values) => createMutation.mutate(values))}>
          <Modal.Header closeButton>
            <Modal.Title>Add Client</Modal.Title>
          </Modal.Header>
          <Modal.Body>
            <ClientFormFields register={form.register} errors={form.formState.errors} users={users} />
          </Modal.Body>
          <Modal.Footer>
            <Button
              variant="outline-secondary"
              onClick={() => {
                setShowAdd(false)
                form.reset(defaultClientFormValues)
              }}
            >
              Cancel
            </Button>
            <Button className="btn-gold" type="submit" disabled={createMutation.isPending}>
              Save client
            </Button>
          </Modal.Footer>
        </Form>
      </Modal>
    </div>
  )
}
