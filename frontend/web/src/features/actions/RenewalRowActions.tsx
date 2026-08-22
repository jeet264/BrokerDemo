import { useState } from 'react'
import { Dropdown } from 'react-bootstrap'
import { Link } from 'react-router-dom'
import { useLanguage } from '../../i18n/LanguageProvider'
import type { SelectedQuotation } from '../../types/api'
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
  clientName: _clientName,
  policyNumber,
  expiryDate,
  premium,
  status,
  selectedQuotation,
}: {
  publicId: string
  clientName?: string
  policyNumber: string
  expiryDate: string
  premium: number
  status: string
  selectedQuotation?: SelectedQuotation | null
}) {
  const { t } = useLanguage()
  const [action, setAction] = useState<RenewalMenuAction>(null)
  const open = isOpenRenewal(status)

  return (
    <>
      <div className="table-actions">
        <Link to={`/renewals/${publicId}`} className="btn btn-sm btn-action-view">
          <i className="bi bi-eye me-1" />
          {t('actions.view')}
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
              <Dropdown.Item onClick={() => setAction('followUp')}>{t('actions.followUp')}</Dropdown.Item>
              <Dropdown.Item onClick={() => setAction('renew')}>{t('actions.markRenewed')}</Dropdown.Item>
              <Dropdown.Item className="text-danger" onClick={() => setAction('lost')}>
                {t('actions.markLost')}
              </Dropdown.Item>
            </Dropdown.Menu>
          </Dropdown>
        )}
      </div>

      <MarkRenewedModal
        show={action === 'renew'}
        onHide={() => setAction(null)}
        publicId={publicId}
        expiryDate={expiryDate}
        premium={premium}
        selectedQuotation={selectedQuotation}
      />
      <MarkLostModal
        show={action === 'lost'}
        onHide={() => setAction(null)}
        publicId={publicId}
        policyNumber={policyNumber}
      />
      <AddFollowUpModal
        show={action === 'followUp'}
        onHide={() => setAction(null)}
        publicId={publicId}
      />
    </>
  )
}
