import { useQuery } from '@tanstack/react-query'
import { fetchPolicies } from '../../api/policies'

function formatInr(amount: number) {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    maximumFractionDigits: 0,
  }).format(amount)
}

export function PoliciesPage() {
  const query = useQuery({
    queryKey: ['policies', 'Active'],
    queryFn: () => fetchPolicies('Active'),
  })

  const policies = query.data?.items ?? []

  return (
    <div>
      <div className="page-heading">
        <div>
          <h2>Policies</h2>
          <p>Active current-term policies only. After a renewal is marked renewed, the new expiry is shown here — never the expired term.</p>
        </div>
      </div>
      <section className="content-card">
        {query.isError && <div className="alert alert-danger">Could not load policies. Sign in and confirm the API is running.</div>}
        {query.isLoading && <p className="text-muted mb-0">Loading policies…</p>}
        {!query.isLoading && policies.length === 0 && <p className="text-muted mb-0">No active policies.</p>}
        {policies.length > 0 && (
          <div className="table-responsive">
            <table className="table align-middle mb-0">
              <thead>
                <tr>
                  <th>Policy</th>
                  <th>Client</th>
                  <th>Insurer</th>
                  <th>Expiry</th>
                  <th>Days</th>
                  <th>Premium</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {policies.map((policy) => (
                  <tr key={policy.publicId}>
                    <td>
                      <strong>{policy.policyNumber}</strong>
                      <div className="text-muted small">{policy.policyType}</div>
                    </td>
                    <td>{policy.clientName}</td>
                    <td>{policy.insurerName}</td>
                    <td>{policy.expiryDate}</td>
                    <td>{policy.daysRemaining}</td>
                    <td>{formatInr(policy.premium)}</td>
                    <td>{policy.status}</td>
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
