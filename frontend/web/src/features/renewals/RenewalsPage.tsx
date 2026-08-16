import { useEffect, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Form } from 'react-bootstrap'
import { useSearchParams } from 'react-router-dom'
import { fetchRenewals, type RenewalDueFilter } from '../../api/renewals'
import { RenewalRowActions } from '../actions'
import { PriorityChip } from '../../components/display/StatusChips'
import { EmptyState, ErrorBanner, LoadingBlock } from '../../components/feedback/PageFeedback'
import { formatDateIn } from '../../lib/format'
import { formatInr } from '../../lib/money'
import { daysShort, stageLabel } from './renewalDisplay'

const FILTERS: { id: RenewalDueFilter; label: string }[] = [
  { id: 'all', label: 'All' },
  { id: 'overdue', label: 'Overdue' },
  { id: 'dueToday', label: 'Due Today' },
  { id: 'dueIn7Days', label: 'Due in 7 Days' },
  { id: 'dueIn30Days', label: 'Due in 30 Days' },
  { id: 'completed', label: 'Completed' },
  { id: 'lost', label: 'Lost' },
]

const VALID_FILTERS = new Set(FILTERS.map((filter) => filter.id))

function parseDue(value: string | null): RenewalDueFilter {
  if (value && VALID_FILTERS.has(value as RenewalDueFilter)) {
    return value as RenewalDueFilter
  }
  return 'all'
}

export function RenewalsPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const dueFilter = parseDue(searchParams.get('due'))
  const [searchInput, setSearchInput] = useState(searchParams.get('q') ?? '')
  const [search, setSearch] = useState(searchParams.get('q') ?? '')

  useEffect(() => {
    const timer = window.setTimeout(() => setSearch(searchInput.trim()), 300)
    return () => window.clearTimeout(timer)
  }, [searchInput])

  const listQuery = useQuery({
    queryKey: ['renewals', dueFilter, search],
    queryFn: () => fetchRenewals({ dueFilter, search: search || undefined, pageSize: 50 }),
  })

  const renewals = listQuery.data?.items ?? []

  const setDueFilter = (id: RenewalDueFilter) => {
    const next = new URLSearchParams(searchParams)
    if (id === 'all') {
      next.delete('due')
    } else {
      next.set('due', id)
    }
    setSearchParams(next, { replace: true })
  }

  return (
    <div>
      <div className="page-heading">
        <h2>Renewals</h2>
        <p>See which policies are expiring, which files are at risk, who owns them, and what to do next.</p>
      </div>

      <section className="content-card mb-3">
        <div className="filter-field mb-3">
          <label htmlFor="renewal-search">Search</label>
          <Form.Control
            id="renewal-search"
            placeholder="Search client, policy, or insurer"
            value={searchInput}
            onChange={(event) => setSearchInput(event.target.value)}
          />
        </div>
        <div className="filter-chips" role="tablist" aria-label="Renewal due filters">
          {FILTERS.map((filter) => (
            <button
              key={filter.id}
              type="button"
              role="tab"
              aria-selected={dueFilter === filter.id}
              className={`filter-chip${dueFilter === filter.id ? ' is-active' : ''}`}
              onClick={() => setDueFilter(filter.id)}
            >
              {filter.label}
            </button>
          ))}
        </div>
      </section>

      <section className="content-card">
        {listQuery.isError && <ErrorBanner>Could not load renewals. Check your connection and try again.</ErrorBanner>}
        {listQuery.isLoading && <LoadingBlock label="Loading renewals…" />}
        {!listQuery.isLoading && renewals.length === 0 && (
          <EmptyState
            icon="bi-arrow-repeat"
            title="No renewals in this view"
            description="Try another due window or search, or open a client to start a renewal."
          />
        )}
        {renewals.length > 0 && (
          <div className="table-responsive table-scroll">
            <table className="table align-middle mb-0">
              <thead>
                <tr>
                  <th>Client</th>
                  <th>Policy</th>
                  <th>Insurer</th>
                  <th className="num">Premium</th>
                  <th>Expiry</th>
                  <th>Days remaining</th>
                  <th>Renewal stage</th>
                  <th>Priority</th>
                  <th>Assigned to</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {renewals.map((renewal) => (
                  <tr key={renewal.publicId} className={renewal.daysRemaining < 0 ? 'row-attention' : undefined}>
                    <td>
                      <strong>{renewal.clientName}</strong>
                    </td>
                    <td>
                      <strong>{renewal.policyNumber}</strong>
                      <div className="text-muted small">{renewal.policyType}</div>
                    </td>
                    <td>{renewal.insurerName}</td>
                    <td className="num">{formatInr(renewal.premium)}</td>
                    <td>{formatDateIn(renewal.expiryDate)}</td>
                    <td className={renewal.daysRemaining <= 0 ? 'is-due-now' : undefined}>
                      {daysShort(renewal.daysRemaining)}
                    </td>
                    <td>{stageLabel(renewal.currentStage)}</td>
                    <td>
                      <PriorityChip priority={renewal.priority} />
                    </td>
                    <td>{renewal.assignedUserName ?? 'Unassigned'}</td>
                    <td>
                      <RenewalRowActions
                        publicId={renewal.publicId}
                        clientName={renewal.clientName}
                        policyNumber={renewal.policyNumber}
                        expiryDate={renewal.expiryDate}
                        premium={renewal.premium}
                        status={renewal.status}
                      />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  )
}
