import { useMemo } from 'react'
import { Button, Form, Modal } from 'react-bootstrap'
import { useForm } from 'react-hook-form'
import { applyApiFieldErrors } from '../../api/client'
import { addDays, addYears } from '../renewals/renewalDisplay'
import { useMarkRenewed } from './useDeskMutations'

interface RenewForm {
  newExpiryDate: string
  premium: number
}

/**
 * Prompt 7B input: next-term expiry and premium. Used from Renewal Detail and the renewals list
 * so both go through `useMarkRenewed` (the rollover path).
 */
export function MarkRenewedModal({
  show,
  publicId,
  expiryDate,
  premium,
  onHide,
}: {
  show: boolean
  publicId: string
  expiryDate: string
  premium: number
  onHide: () => void
}) {
  const markRenewed = useMarkRenewed()
  const defaults = useMemo<RenewForm>(() => {
    const nextStart = addDays(expiryDate, 1)
    return { newExpiryDate: addYears(nextStart, 1), premium }
  }, [expiryDate, premium])
  const form = useForm<RenewForm>({ values: defaults })

  return (
    <Modal show={show} onHide={onHide} centered>
      <Form
        onSubmit={form.handleSubmit((values) =>
          markRenewed.mutate(
            { publicId, newExpiryDate: values.newExpiryDate, premium: values.premium },
            {
              onSuccess: onHide,
              onError: (error) => applyApiFieldErrors(error, form.setError),
            },
          ),
        )}
      >
        <Modal.Header closeButton>
          <Modal.Title>Mark renewed</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <p className="text-muted">
            Creates the next-term policy starting the day after the current expiry. The old policy is kept as Expired.
          </p>
          <Form.Group className="mb-3">
            <Form.Label>New expiry date</Form.Label>
            <Form.Control type="date" {...form.register('newExpiryDate', { required: true })} />
          </Form.Group>
          <Form.Group>
            <Form.Label>Premium</Form.Label>
            <Form.Control
              type="number"
              step="0.01"
              min="0"
              {...form.register('premium', { required: true, valueAsNumber: true })}
            />
          </Form.Group>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="outline-secondary" onClick={onHide}>
            Cancel
          </Button>
          <Button className="btn-gold" type="submit" disabled={markRenewed.isPending}>
            Confirm
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  )
}
