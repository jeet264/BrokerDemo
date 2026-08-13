import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Tab, Tabs } from 'react-bootstrap'
import { Link, useParams } from 'react-router-dom'
import { fetchClient, fetchClientActivities, fetchClientPolicies, fetchClientRenewals } from '../../api/clients'
import { formatInr } from '../../lib/money'
import type { ClientActivity, ClientPolicy, ClientRenewal } from '../../types/api'

function formatIst(utcIso: string) {
  return new Intl.DateTimeFormat('en-IN', {
    dateStyle: 'medium',
    timeStyle: 'short',
    timeZone: 'Asia/Kolkata',
  }).format(new Date(utcIso))
}

function priorityChip(priority: string) {
  const key = priority.toLowerCase()
  const className =
    key === 'critical'
      ? 'priority-chip priority-chip-critical'
      : key === 'high'
        ? 'priority-chip priority-chip-high'
        : key === 'low'
          ? 'priority-chip priority-chip-low'
          : 'priority-chip priority-chip-medium'
  return <span className={className}>{priority}</span>
}

export function ClientDetailPage() {
  const { publicId = '' } = useParams()
  const [tab, setTab] = useState('overview')

  const clientQuery = useQuery({
    queryKey: ['client', publicId],
    queryFn: () => fetchClient(publicId),
    enabled: Boolean(publicId),
  })

  const policiesQuery = useQuery({
    queryKey: ['client-policies', publicId],
    queryFn: () => fetchClientPolicies(publicId),
    enabled: Boolean(publicId) && tab === 'policies',
  })

  const renewalsQuery = useQuery({
    queryKey: ['client-renewals', publicId],
    queryFn: () => fetchClientRenewals(publicId),
    enabled: Boolean(publicId) && tab === 'renewals',
  })

  const activitiesQuery = useQuery({
    queryKey: ['client-activities', publicId],
    queryFn: () => fetchClientActivities(publicId),
    enabled: Boolean(publicId) && tab === 'activities',
  })

  const client = clientQuery.data

  if (clientQuery.isError) {
    return (
      <div>
        <div className="page-heading">
          <Link to="/clients" className="text-decoration-none">
            ← Clients
          </Link>
          <h2 className="mt-2">Client not found</h2>
        </div>
        <div className="alert alert-danger">This client is not in your book, or the API could not be reached.</div>
      </div>
    )
  }

  if (!client) {
    return (
      <div>
        <div className="page-heading">
          <h2>Client</h2>
          <p className="text-muted">Loading client…</p>
        </div>
      </div>
    )
  }

  return (
    <div>
      <div className="page-heading d-flex justify-content-between align-items-start gap-3">
        <div>
          <Link to="/clients" className="text-decoration-none">
            ← Clients
          </Link>
          <h2 className="mt-2 mb-1">{client.companyName}</h2>
          <p className="mb-0">
            {client.clientCode} · {client.clientType}
            {client.industry ? ` · ${client.industry}` : ''}
          </p>
        </div>
        <span className={client.isActive ? 'status-pill' : 'priority-chip priority-chip-low'}>
          {client.isActive ? 'Active' : 'Inactive'}
        </span>
      </div>

      <div className="metric-grid metric-grid-four mb-4">
        <article className="metric-card">
          <span className="metric-label">Policies</span>
          <strong>{client.policyCount}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">Active policies</span>
          <strong>{client.activePolicyCount}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">Upcoming renewals</span>
          <strong>{client.upcomingRenewalCount}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">Total premium</span>
          <strong>{formatInr(client.totalPremium)}</strong>
        </article>
      </div>

      <section className="content-card">
        <Tabs activeKey={tab} onSelect={(next) => setTab(next ?? 'overview')} className="mb-3">
          <Tab eventKey="overview" title="Overview">
            <div className="row g-4">
              <div className="col-md-6">
                <h3 className="h6 text-uppercase text-muted">Contact information</h3>
                <p className="mb-1">{client.email}</p>
                <p className="mb-1">{client.phone}</p>
                {client.alternatePhone && <p className="mb-1">{client.alternatePhone}</p>}
                <p className="mb-0">
                  {client.addressLine1}
                  {client.addressLine2 ? `, ${client.addressLine2}` : ''}
                  <br />
                  {client.city}, {client.state} {client.postalCode}
                  <br />
                  {client.country}
                </p>
              </div>
              <div className="col-md-6">
                <h3 className="h6 text-uppercase text-muted">Assigned broker</h3>
                <p className="mb-3">{client.assignedUserName ?? 'Unassigned'}</p>
                {client.notes && (
                  <>
                    <h3 className="h6 text-uppercase text-muted">Notes</h3>
                    <p className="mb-0">{client.notes}</p>
                  </>
                )}
              </div>
            </div>
          </Tab>
          <Tab eventKey="policies" title="Policies">
            <PolicyTable policies={policiesQuery.data ?? []} loading={policiesQuery.isLoading} />
          </Tab>
          <Tab eventKey="renewals" title="Renewals">
            <RenewalTable renewals={renewalsQuery.data ?? []} loading={renewalsQuery.isLoading} />
          </Tab>
          <Tab eventKey="activities" title="Activities">
            <ActivityList activities={activitiesQuery.data ?? []} loading={activitiesQuery.isLoading} />
          </Tab>
        </Tabs>
      </section>
    </div>
  )
}

function PolicyTable({ policies, loading }: { policies: ClientPolicy[]; loading: boolean }) {
  if (loading) {
    return <p className="text-muted mb-0">Loading policies…</p>
  }
  if (policies.length === 0) {
    return <p className="text-muted mb-0">No policies for this client.</p>
  }
  return (
    <div className="table-responsive">
      <table className="table align-middle mb-0">
        <thead>
          <tr>
            <th>Policy</th>
            <th>Insurer</th>
            <th>Premium</th>
            <th>Expiry</th>
            <th>Status</th>
            <th>Assigned to</th>
          </tr>
        </thead>
        <tbody>
          {policies.map((policy) => (
            <tr key={policy.publicId}>
              <td>
                <Link to={`/policies/${policy.publicId}`}>
                  <strong>{policy.policyNumber}</strong>
                </Link>
                <div className="text-muted small">{policy.policyType}</div>
              </td>
              <td>{policy.insurerName ?? '—'}</td>
              <td>{formatInr(policy.premium)}</td>
              <td>{policy.expiryDate}</td>
              <td>{policy.status}</td>
              <td>{policy.assignedUserName ?? '—'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function RenewalTable({ renewals, loading }: { renewals: ClientRenewal[]; loading: boolean }) {
  if (loading) {
    return <p className="text-muted mb-0">Loading renewals…</p>
  }
  if (renewals.length === 0) {
    return <p className="text-muted mb-0">No renewals for this client.</p>
  }
  return (
    <div className="table-responsive">
      <table className="table align-middle mb-0">
        <thead>
          <tr>
            <th>Policy</th>
            <th>Renewal date</th>
            <th>Status</th>
            <th>Priority</th>
            <th>Stage</th>
            <th>Assigned to</th>
          </tr>
        </thead>
        <tbody>
          {renewals.map((renewal) => (
            <tr key={renewal.publicId}>
              <td>{renewal.policyNumber}</td>
              <td>{renewal.renewalDate}</td>
              <td>{renewal.status}</td>
              <td>{priorityChip(renewal.priority)}</td>
              <td>{renewal.currentStage}</td>
              <td>{renewal.assignedUserName ?? '—'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function ActivityList({ activities, loading }: { activities: ClientActivity[]; loading: boolean }) {
  if (loading) {
    return <p className="text-muted mb-0">Loading activity…</p>
  }
  if (activities.length === 0) {
    return <p className="text-muted mb-0">No activity recorded yet.</p>
  }
  return (
    <div className="table-responsive">
      <table className="table align-middle mb-0">
        <thead>
          <tr>
            <th>When</th>
            <th>Type</th>
            <th>Description</th>
            <th>User</th>
          </tr>
        </thead>
        <tbody>
          {activities.map((activity) => (
            <tr key={activity.publicId}>
              <td>{formatIst(activity.createdAtUtc)}</td>
              <td>{activity.activityType}</td>
              <td>{activity.description}</td>
              <td>{activity.userName ?? '—'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
