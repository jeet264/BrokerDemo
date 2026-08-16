import { useState } from 'react'
import { Dropdown } from 'react-bootstrap'
import { Link } from 'react-router-dom'
import { isOpenRenewal } from '../renewals/renewalDisplay'
import { AddFollowUpModal } from './AddFollowUpModal'
import { MarkLostModal } from './MarkLostModal'
import { MarkRenewedModal } from './MarkRenewedModal'

type RenewalMenuAction = 'followUp' | 'renew' | 'lost' | null

/**
 * Compact kebab for a renewal row. Same mutations as Renewal Detail (via useMarkRenewed /
 * useMarkLost / useAddFollowUp) so list and detail cannot drift.
 */
export function RenewalRowActions({
  publicId,
  clientName,
  policyNumber,
  expiryDate,
  premium,
  status,
}: {
  publicId: string
  clientName: string
  policyNumber: string
  expiryDate: string
  premium: number
  status: string
}) {
  const [action, setAction] = useState<RenewalMenuAction>(null)
  const open = isOpenRenewal(status)

  return (
    <>
      <div className="table-actions">
        <Link to={`/renewals/${publicId}`} className="btn btn-sm btn-outline-secondary">
          View
        </Link>
        {open && (
          <Dropdown align="end">
            <Dropdown.Toggle
              variant="outline-secondary"
              size="sm"
              className="row-kebab-toggle"
              id={`renewal-actions-${publicId}`}
            >
              <span className="visually-hidden">More actions</span>
              <i className="bi bi-three-dots-vertical" aria-hidden />
            </Dropdown.Toggle>
            <Dropdown.Menu>
              <Dropdown.Item onClick={() => setAction('followUp')}>Add Follow-up</Dropdown.Item>
              <Dropdown.Item onClick={() => setAction('renew')}>Mark Renewed</Dropdown.Item>
              <Dropdown.Item className="text-danger" onClick={() => setAction('lost')}>
                Mark Lost
              </Dropdown.Item>
            </Dropdown.Menu>
          </Dropdown>
        )}
      </div>
      <AddFollowUpModal
        show={action === 'followUp'}
        publicId={publicId}
        defaultDescription={`Follow up with ${clientName} on ${policyNumber}.`}
        onHide={() => setAction(null)}
      />
      <MarkRenewedModal
        show={action === 'renew'}
        publicId={publicId}
        expiryDate={expiryDate}
        premium={premium}
        onHide={() => setAction(null)}
      />
      <MarkLostModal
        show={action === 'lost'}
        publicId={publicId}
        policyNumber={policyNumber}
        onHide={() => setAction(null)}
      />
    </>
  )
}
