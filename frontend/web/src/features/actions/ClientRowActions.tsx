import { Link } from 'react-router-dom'

/**
 * Client-list shortcuts that do not need a detail page: dial the phone, or jump to that client's policies.
 */
export function ClientRowActions({
  publicId,
  companyName,
  phone,
}: {
  publicId: string
  companyName: string
  phone: string
}) {
  const tel = phone.replace(/\s+/g, '')

  return (
    <div className="table-actions">
      {tel ? (
        <a className="btn btn-sm btn-gold" href={`tel:${tel}`} aria-label={`Call ${companyName}`}>
          Call
        </a>
      ) : null}
      <Link to={`/policies?client=${publicId}`} className="btn btn-sm btn-outline-secondary">
        View Policies
      </Link>
      <Link to={`/clients/${publicId}`} className="btn btn-sm btn-outline-secondary">
        View
      </Link>
    </div>
  )
}
