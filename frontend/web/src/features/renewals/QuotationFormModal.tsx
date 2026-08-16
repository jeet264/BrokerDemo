import { useEffect } from 'react'
import { Button, Form, Modal } from 'react-bootstrap'
import { useForm } from 'react-hook-form'
import { useQuery } from '@tanstack/react-query'
import { applyApiFieldErrors } from '../../api/client'
import { fetchInsurers } from '../../api/insurers'
import type { QuotationWriteBody } from '../../api/quotations'
import type { Quotation } from '../../types/api'

interface QuotationForm {
  insurerMode: 'existing' | 'new'
  insurerPublicId: string
  newInsurerName: string
  premiumAmount: number
  sumInsured: string
  coverageSummary: string
  validUntil: string
  notes: string
}

export function QuotationFormModal({
  show,
  quotation,
  onHide,
  onSubmit,
  isPending,
}: {
  show: boolean
  quotation: Quotation | null
  onHide: () => void
  onSubmit: (body: QuotationWriteBody) => Promise<unknown>
  isPending: boolean
}) {
  const insurersQuery = useQuery({
    queryKey: ['insurers'],
    queryFn: fetchInsurers,
    enabled: show,
  })
  const form = useForm<QuotationForm>({
    defaultValues: emptyForm(),
  })
  const insurerMode = form.watch('insurerMode')

  useEffect(() => {
    if (!show) {
      return
    }
    form.reset(quotation ? formFromQuotation(quotation) : emptyForm())
  }, [show, quotation, form])

  return (
    <Modal show={show} onHide={onHide} centered>
      <Form
        onSubmit={form.handleSubmit(async (values) => {
          try {
            await onSubmit(toWriteBody(values))
            onHide()
          } catch (error) {
            applyApiFieldErrors(error, form.setError)
          }
        })}
      >
        <Modal.Header closeButton>
          <Modal.Title>{quotation ? 'Edit quotation' : 'Add quotation'}</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <p className="text-muted small">
            Log what the insurer quoted on the call or email. This is not an electronic request to the
            insurer.
          </p>
          <Form.Group className="mb-3">
            <Form.Label>Insurer</Form.Label>
            <Form.Check
              type="radio"
              id="quote-insurer-existing"
              label="Choose from the list"
              value="existing"
              {...form.register('insurerMode')}
            />
            <Form.Check
              type="radio"
              id="quote-insurer-new"
              label="Type a new insurer name"
              value="new"
              {...form.register('insurerMode')}
            />
          </Form.Group>
          {insurerMode === 'existing' ? (
            <Form.Group className="mb-3">
              <Form.Label>Insurer</Form.Label>
              <Form.Select {...form.register('insurerPublicId', { required: insurerMode === 'existing' })}>
                <option value="">Select insurer</option>
                {(insurersQuery.data ?? []).map((insurer) => (
                  <option key={insurer.publicId} value={insurer.publicId}>
                    {insurer.name}
                  </option>
                ))}
              </Form.Select>
            </Form.Group>
          ) : (
            <Form.Group className="mb-3">
              <Form.Label>New insurer name</Form.Label>
              <Form.Control
                {...form.register('newInsurerName', { required: insurerMode === 'new' })}
                placeholder="e.g. Tata AIG"
              />
            </Form.Group>
          )}
          <Form.Group className="mb-3">
            <Form.Label>Premium</Form.Label>
            <Form.Control
              type="number"
              step="0.01"
              min="0"
              {...form.register('premiumAmount', { required: true, valueAsNumber: true })}
            />
          </Form.Group>
          <Form.Group className="mb-3">
            <Form.Label>Sum insured (optional)</Form.Label>
            <Form.Control type="number" step="0.01" min="0" {...form.register('sumInsured')} />
          </Form.Group>
          <Form.Group className="mb-3">
            <Form.Label>Coverage summary</Form.Label>
            <Form.Control
              as="textarea"
              rows={2}
              maxLength={1000}
              {...form.register('coverageSummary')}
              placeholder="Short description of what this quote covers"
            />
          </Form.Group>
          <Form.Group className="mb-3">
            <Form.Label>Valid until (optional)</Form.Label>
            <Form.Control type="date" {...form.register('validUntil')} />
          </Form.Group>
          <Form.Group>
            <Form.Label>Notes (optional)</Form.Label>
            <Form.Control
              as="textarea"
              rows={2}
              maxLength={2000}
              {...form.register('notes')}
              placeholder="e.g. insurer said no-claim bonus applies"
            />
          </Form.Group>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="outline-secondary" onClick={onHide}>
            Cancel
          </Button>
          <Button className="btn-gold" type="submit" disabled={isPending}>
            {quotation ? 'Save quotation' : 'Add quotation'}
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  )
}

function emptyForm(): QuotationForm {
  return {
    insurerMode: 'existing',
    insurerPublicId: '',
    newInsurerName: '',
    premiumAmount: 0,
    sumInsured: '',
    coverageSummary: '',
    validUntil: '',
    notes: '',
  }
}

function formFromQuotation(quotation: Quotation): QuotationForm {
  return {
    insurerMode: 'existing',
    insurerPublicId: quotation.insurerPublicId,
    newInsurerName: '',
    premiumAmount: quotation.premiumAmount,
    sumInsured: quotation.sumInsured == null ? '' : String(quotation.sumInsured),
    coverageSummary: quotation.coverageSummary,
    validUntil: quotation.validUntil ?? '',
    notes: quotation.notes ?? '',
  }
}

function toWriteBody(values: QuotationForm): QuotationWriteBody {
  const sumInsured = values.sumInsured === '' ? null : Number(values.sumInsured)
  return {
    insurerPublicId: values.insurerMode === 'existing' ? values.insurerPublicId : undefined,
    newInsurerName: values.insurerMode === 'new' ? values.newInsurerName.trim() : undefined,
    premiumAmount: values.premiumAmount,
    sumInsured: Number.isFinite(sumInsured) ? sumInsured : null,
    coverageSummary: values.coverageSummary.trim() || undefined,
    validUntil: values.validUntil || null,
    notes: values.notes.trim() || null,
  }
}
