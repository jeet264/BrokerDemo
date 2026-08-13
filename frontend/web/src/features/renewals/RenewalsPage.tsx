import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { fetchRenewals, type RenewalDueFilter } from '../../api/renewals'
import { formatInr } from '../../lib/money'
import { daysShort, priorityClass, stageLabel } from './renewalDisplay'

const FILTERS: { id: RenewalDueFilter; label: string }[] = [
  { id: 'all', label: 'All' },
  { id: 'overdue', label: 'Overdue' },
  { id: 'dueToday', label: 'Due Today' },
  { id: 'dueIn7Days', label: 'Due in 7 Days' },
  { id: 'dueIn30Days', label: 'Due in 30 Days' },
  { id: 'completed', label: 'Completed' },
  { id: 'lost', label: 'Lost' },
]

export function RenewalsPage() {
  const [dueFilter, setDueFilter] = useState<RenewalDueFilter>('all')

  const listQuery = useQuery({
    queryKey: ['renewals', dueFilter],
    queryFn: () => fetchRenewals({ dueFilter, pageSize: 50 }),
  })

  const renewals = listQuery.data?.items ?? []

  return (
    <div>
      <div className="page-heading">
        <h2>Renewals</h2>
        <p>Work the book by due window. Open a renewal to contact the client, follow up, and roll the term.</p>
      </div>

      <section className="content-card mb-3">
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
        {listQuery.isError && <div className="alert alert-danger">Could not load renewals. Sign in and confirm the API is running.</div>}
        {listQuery.isLoading && <p className="text-muted mb-0">Loading renewals…</p>}
        {!listQuery.isLoading && renewals.length === 0 && <p className="text-muted mb-0">No renewals in this view.</p>}
        {renewals.length > 0 && (
          <div className="table-responsive">
            <table className="table align-middle mb-0">
              <thead>
                <tr>
                  <th>Client</th>
                  <th>Policy</th>
                  <th>Insurer</th>
                  <th>Premium</th>
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
                    <td>{formatInr(renewal.premium)}</td>
                    <td>{renewal.expiryDate}</td>
                    <td className={renewal.daysRemaining <= 0 ? 'is-due-now' : undefined}>{daysShort(renewal.daysRemaining)}</td>
                    <td>{stageLabel(renewal.currentStage)}</td>
                    <td>
                      <span className={priorityClass(renewal.priority)}>{renewal.priority}</span>
                    </td>
                    <td>{renewal.assignedUserName ?? 'Unassigned'}</td>
                    <td>
                      <div className="table-actions">
                        <Link to={`/renewals/${renewal.publicId}`} className="btn btn-sm btn-outline-secondary">
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
    </div>
  )
}
