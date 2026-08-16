import type { ReactNode } from 'react'
import { Button, Modal } from 'react-bootstrap'

/**
 * One-click confirm for significant desk actions (Mark Lost, and optional Complete).
 * No extra form fields — if an action needs input (Mark Renewed expiry/premium), use that modal instead.
 */
export function ConfirmActionModal({
  show,
  title,
  body,
  confirmLabel,
  confirmVariant = 'danger',
  pending,
  onHide,
  onConfirm,
}: {
  show: boolean
  title: string
  body: ReactNode
  confirmLabel: string
  confirmVariant?: 'danger' | 'primary'
  pending: boolean
  onHide: () => void
  onConfirm: () => void
}) {
  return (
    <Modal show={show} onHide={onHide} centered>
      <Modal.Header closeButton>
        <Modal.Title>{title}</Modal.Title>
      </Modal.Header>
      <Modal.Body>{body}</Modal.Body>
      <Modal.Footer>
        <Button variant="outline-secondary" onClick={onHide}>
          Keep
        </Button>
        <Button
          variant={confirmVariant === 'danger' ? 'danger' : undefined}
          className={confirmVariant === 'primary' ? 'btn-gold' : undefined}
          onClick={onConfirm}
          disabled={pending}
        >
          {confirmLabel}
        </Button>
      </Modal.Footer>
    </Modal>
  )
}
