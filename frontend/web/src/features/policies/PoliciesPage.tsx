import { Link } from 'react-router-dom'

/**
 * Policy book landing. There is no standalone policy list API yet — imported terms hang off each client.
 * This page exists so a broker can import cover from Excel without waiting on that list screen.
 */
export function PoliciesPage() {
  return (
    <div>
      <div className="page-heading d-flex justify-content-between align-items-start gap-3 flex-wrap">
        <div>
          <h2>Policies</h2>
          <p>
            Each row is one policy term. Import matches rows to clients already in BrokerOS (by client code, or by name
            and phone). Import clients first if the spreadsheet is a full book of business.
          </p>
        </div>
        <Link className="btn btn-gold" to="/policies/import">
          Import from Excel/CSV
        </Link>
      </div>
      <section className="content-card empty-state">
        <i className="bi bi-file-earmark-text" />
        <h3>Bring in the existing book</h3>
        <p>Use the template so PolicyNumber, dates, premium, and insurer line up. Invalid rows are previewed, not saved.</p>
        <Link className="btn btn-gold mt-2" to="/policies/import">
          Start import
        </Link>
      </section>
    </div>
  )
}
