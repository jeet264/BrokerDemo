import { Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { fetchClients } from '../../api/import'

/**
 * Client book for the current brokerage. Lists records from GET /api/clients.
 * Import from Excel/CSV is the path for an existing 100–300 policy book — typing each client by hand is not.
 */
export function ClientsPage() {
  const listQuery = useQuery({
    queryKey: ['clients', 'list'],
    queryFn: () => fetchClients(1, 50),
  })

  const items = listQuery.data?.items ?? []

  return (
    <div>
      <div className="page-heading d-flex justify-content-between align-items-start gap-3 flex-wrap">
        <div>
          <h2>Clients</h2>
          <p>The buyers of insurance at this brokerage. Import the existing Excel book, then attach policies.</p>
        </div>
        <Link className="btn btn-gold" to="/clients/import">
          Import from Excel/CSV
        </Link>
      </div>

      {listQuery.isError && (
        <div className="alert alert-danger" role="alert">
          {listQuery.error instanceof Error
            ? listQuery.error.message
            : 'Could not load clients. Sign in as a broker user and confirm the API is running.'}
        </div>
      )}

      <section className="content-card">
        {listQuery.isLoading ? (
          <p className="mb-0 text-muted">
            <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true" />
            Loading clients…
          </p>
        ) : items.length === 0 ? (
          <div className="empty-state">
            <i className="bi bi-people" />
            <h3>No clients yet</h3>
            <p>Download the template and import the brokerage’s existing client list.</p>
          </div>
        ) : (
          <div className="table-responsive">
            <table className="table align-middle">
              <thead>
                <tr>
                  <th>Code</th>
                  <th>Name</th>
                  <th>Type</th>
                  <th>Phone</th>
                  <th>City</th>
                  <th>Owner</th>
                </tr>
              </thead>
              <tbody>
                {items.map((client) => (
                  <tr key={client.publicId}>
                    <td>{client.clientCode}</td>
                    <td>{client.companyName}</td>
                    <td>{client.clientType}</td>
                    <td>{client.phone}</td>
                    <td>
                      {client.city}, {client.state}
                    </td>
                    <td>{client.assignedUserName ?? 'Unassigned'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            {listQuery.data && listQuery.data.totalCount > items.length && (
              <p className="text-muted mb-0">Showing {items.length} of {listQuery.data.totalCount} clients.</p>
            )}
          </div>
        )}
      </section>
    </div>
  )
}
