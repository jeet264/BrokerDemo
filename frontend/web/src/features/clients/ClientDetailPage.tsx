import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Tab, Tabs } from 'react-bootstrap'
import { Link, useParams } from 'react-router-dom'
import { fetchClient, fetchClientActivities, fetchClientPolicies, fetchClientRenewals } from '../../api/clients'
import { PriorityChip, StatusChip } from '../../components/display/StatusChips'
import { EmptyState, ErrorBanner, LoadingBlock } from '../../components/feedback/PageFeedback'
import { formatDateIn, formatDateTimeIst, humanizeEnum, telHref } from '../../lib/format'
import { formatInr } from '../../lib/money'
import { stageLabel } from '../renewals/renewalDisplay'
import type { ClientActivity, ClientPolicy, ClientRenewal } from '../../types/api'

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
        <ErrorBanner>This client is not in your book, or the API could not be reached.</ErrorBanner>
      </div>
    )
  }

  if (!client) {
    return (
      <div>
        <div className="page-heading">
          <h2>Client</h2>
        </div>
        <LoadingBlock label="Loading client…" />
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
                <dl className="detail-list mb-0">
                  <div>
                    <dt>Email</dt>
                    <dd>
                      <a className="contact-link" href={`mailto:${client.email}`}>
                        {client.email}
                      </a>
                    </dd>
                  </div>
                  <div>
                    <dt>Phone</dt>
                    <dd>
                      <a className="contact-link" href={telHref(client.phone)}>
                        {client.phone}
                      </a>
                    </dd>
                  </div>
                  {client.alternatePhone && (
                    <div>
                      <dt>Alternate</dt>
                      <dd>
                        <a className="contact-link" href={telHref(client.alternatePhone)}>
                          {client.alternatePhone}
                        </a>
                      </dd>
                    </div>
                  )}
                  <div>
                    <dt>Address</dt>
                    <dd>
                      {client.addressLine1}
                      {client.addressLine2 ? `, ${client.addressLine2}` : ''}
                      <br />
                      {client.city}, {client.state} {client.postalCode}
                      <br />
                      {client.country}
                    </dd>
                  </div>
                </dl>
              </div>
              <div className="col-md-6">
                <h3 className="h6 text-uppercase text-muted">Account</h3>
                <dl className="detail-list mb-0">
                  <div>
                    <dt>Assigned broker</dt>
                    <dd>{client.assignedUserName ?? 'Unassigned'}</dd>
                  </div>
                  <div>
                    <dt>On books since</dt>
                    <dd>{formatDateTimeIst(client.createdAtUtc)}</dd>
                  </div>
                  {client.notes && (
                    <div>
                      <dt>Notes</dt>
                      <dd>{client.notes}</dd>
                    </div>
                  )}
                </dl>
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
    return <LoadingBlock label="Loading policies…" />
  }
  if (policies.length === 0) {
    return (
      <EmptyState
        icon="bi-file-earmark-text"
        title="No policies for this client"
        description="Add a policy from the policies book to start tracking expiry and renewal."
      />
    )
  }
  return (
    <div className="table-responsive">
      <table className="table align-middle mb-0">
        <thead>
          <tr>
            <th>Policy</th>
            <th>Insurer</th>
            <th className="num">Premium</th>
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
                <div className="text-muted small">{humanizeEnum(policy.policyType)}</div>
              </td>
              <td>{policy.insurerName ?? '—'}</td>
              <td className="num">{formatInr(policy.premium)}</td>
              <td>{formatDateIn(policy.expiryDate)}</td>
              <td>
                <StatusChip status={policy.status} />
              </td>
              <td>{policy.assignedUserName ?? 'Unassigned'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function RenewalTable({ renewals, loading }: { renewals: ClientRenewal[]; loading: boolean }) {
  if (loading) {
    return <LoadingBlock label="Loading renewals…" />
  }
  if (renewals.length === 0) {
    return (
      <EmptyState
        icon="bi-arrow-repeat"
        title="No renewals for this client"
        description="Renewals appear here when a policy is on the book and approaching expiry."
      />
    )
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
              <td>
                <Link to={`/renewals/${renewal.publicId}`}>{renewal.policyNumber}</Link>
              </td>
              <td>{formatDateIn(renewal.renewalDate)}</td>
              <td>
                <StatusChip status={renewal.status} />
              </td>
              <td>
                <PriorityChip priority={renewal.priority} />
              </td>
              <td>{stageLabel(renewal.currentStage)}</td>
              <td>{renewal.assignedUserName ?? 'Unassigned'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function ActivityList({ activities, loading }: { activities: ClientActivity[]; loading: boolean }) {
  if (loading) {
    return <LoadingBlock label="Loading activity…" />
  }
  if (activities.length === 0) {
    return (
      <EmptyState
        icon="bi-clock-history"
        title="No activity recorded yet"
        description="Calls, notes, and renewal updates for this client will appear here."
      />
    )
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
              <td>{formatDateTimeIst(activity.createdAtUtc)}</td>
              <td>{humanizeEnum(activity.activityType)}</td>
              <td>{activity.description}</td>
              <td>{activity.userName ?? '—'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
