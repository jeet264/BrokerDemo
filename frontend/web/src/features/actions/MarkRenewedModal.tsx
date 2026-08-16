import { useMemo } from 'react'
import { Button, Form, Modal } from 'react-bootstrap'
import { useForm } from 'react-hook-form'
import { useQuery } from '@tanstack/react-query'
import { applyApiFieldErrors } from '../../api/client'
import { fetchInsurers } from '../../api/insurers'
import { addDays, addYears } from '../renewals/renewalDisplay'
import { useMarkRenewed } from './useDeskMutations'
import type { SelectedQuotation } from '../../types/api'
import { formatInr } from '../../lib/money'

interface RenewForm {
  newExpiryDate: string
  premium: number
  insurerPublicId: string
  sumInsured: string
}

/**
 * Prompt 7B input: next-term expiry and premium. Used from Renewal Detail and the renewals list
 * so both go through `useMarkRenewed` (the rollover path).
 *
 * When a quotation is Selected on the file, premium and insurer are pre-filled from that quote
 * (the source of truth for what the client is renewing into). The broker can still edit before
 * confirming. With no selected quote the form is unchanged: expiry + current-term premium.
 */
export function MarkRenewedModal({
  show,
  publicId,
  expiryDate,
  premium,
  selectedQuotation,
  onHide,
}: {
  show: boolean
  publicId: string
  expiryDate: string
  premium: number
  selectedQuotation?: SelectedQuotation | null
  onHide: () => void
}) {
  const markRenewed = useMarkRenewed()
  const fromQuote = Boolean(selectedQuotation)
  const insurersQuery = useQuery({
    queryKey: ['insurers'],
    queryFn: fetchInsurers,
    enabled: show && fromQuote,
  })
  const defaults = useMemo<RenewForm>(() => {
    const nextStart = addDays(expiryDate, 1)
    return {
      newExpiryDate: addYears(nextStart, 1),
      premium: selectedQuotation?.premiumAmount ?? premium,
      insurerPublicId: selectedQuotation?.insurerPublicId ?? '',
      sumInsured: selectedQuotation?.sumInsured == null ? '' : String(selectedQuotation.sumInsured),
    }
  }, [expiryDate, premium, selectedQuotation])
  const form = useForm<RenewForm>({ values: defaults })

  return (
    <Modal show={show} onHide={onHide} centered>
      <Form
        onSubmit={form.handleSubmit((values) => {
          const sumInsured = values.sumInsured === '' ? undefined : Number(values.sumInsured)
          markRenewed.mutate(
            {
              publicId,
              newExpiryDate: values.newExpiryDate,
              premium: values.premium,
              insurerPublicId: fromQuote && values.insurerPublicId ? values.insurerPublicId : undefined,
              sumInsured: Number.isFinite(sumInsured) ? sumInsured : undefined,
            },
            {
              onSuccess: onHide,
              onError: (error) => applyApiFieldErrors(error, form.setError),
            },
          )
        })}
      >
        <Modal.Header closeButton>
          <Modal.Title>Mark renewed</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <p className="text-muted">
            Creates the next-term policy starting the day after the current expiry. The old policy is kept as Expired.
          </p>
          {fromQuote && (
            <p className="small mb-3" data-testid="prefill-from-quotation">
              Pre-filled from selected quotation
              {selectedQuotation
                ? ` (${selectedQuotation.insurerName} — ${formatInr(selectedQuotation.premiumAmount)})`
                : ''}
              . You can still edit before confirming.
            </p>
          )}
          <Form.Group className="mb-3">
            <Form.Label htmlFor="renew-expiry">New expiry date</Form.Label>
            <Form.Control
              type="date"
              id="renew-expiry"
              {...form.register('newExpiryDate', { required: true })}
            />
          </Form.Group>
          <Form.Group className={fromQuote ? 'mb-3' : undefined}>
            <Form.Label htmlFor="renew-premium">Premium</Form.Label>
            <Form.Control
              type="number"
              step="0.01"
              min="0"
              id="renew-premium"
              {...form.register('premium', { required: true, valueAsNumber: true })}
            />
          </Form.Group>
          {fromQuote && (
            <>
              <Form.Group className="mb-3">
                <Form.Label htmlFor="renew-insurer">Insurer</Form.Label>
                <Form.Select id="renew-insurer" {...form.register('insurerPublicId', { required: true })}>
                  <option value="">Select insurer</option>
                  {(insurersQuery.data ?? []).map((insurer) => (
                    <option key={insurer.publicId} value={insurer.publicId}>
                      {insurer.name}
                    </option>
                  ))}
                </Form.Select>
              </Form.Group>
              <Form.Group>
                <Form.Label htmlFor="renew-sum-insured">Sum insured (optional)</Form.Label>
                <Form.Control
                  id="renew-sum-insured"
                  type="number"
                  step="0.01"
                  min="0"
                  {...form.register('sumInsured')}
                />
              </Form.Group>
            </>
          )}
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
