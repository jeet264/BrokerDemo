import { Button, Form, Modal } from 'react-bootstrap'
import { useForm } from 'react-hook-form'
import { applyApiFieldErrors } from '../../api/client'
import { FOLLOW_UP_TYPES, istDateToUtc, tomorrowIsoDate } from '../renewals/renewalDisplay'
import { useAddFollowUp } from './useDeskMutations'

interface FollowUpForm {
  activityType: string
  description: string
  nextFollowUpDate: string
}

export function AddFollowUpModal({
  show,
  publicId,
  defaultDescription,
  onHide,
}: {
  show: boolean
  publicId: string
  defaultDescription?: string
  onHide: () => void
}) {
  const addFollowUp = useAddFollowUp()
  const form = useForm<FollowUpForm>({
    values: {
      activityType: 'Call',
      description: defaultDescription ?? '',
      nextFollowUpDate: tomorrowIsoDate(),
    },
  })

  return (
    <Modal show={show} onHide={onHide} centered>
      <Form
        onSubmit={form.handleSubmit((values) =>
          addFollowUp.mutate(
            {
              publicId,
              activityType: values.activityType,
              description: values.description.trim(),
              nextFollowUpAtUtc: values.nextFollowUpDate ? istDateToUtc(values.nextFollowUpDate) : undefined,
            },
            {
              onSuccess: onHide,
              onError: (error) => applyApiFieldErrors(error, form.setError),
            },
          ),
        )}
      >
        <Modal.Header closeButton>
          <Modal.Title>Add follow-up</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form.Group className="mb-3">
            <Form.Label>Follow-up type</Form.Label>
            <Form.Select {...form.register('activityType', { required: 'Follow-up type is required' })}>
              {FOLLOW_UP_TYPES.map((type) => (
                <option key={type.id} value={type.id}>
                  {type.label}
                </option>
              ))}
            </Form.Select>
          </Form.Group>
          <Form.Group className="mb-3">
            <Form.Label>Description</Form.Label>
            <Form.Control
              as="textarea"
              rows={3}
              isInvalid={Boolean(form.formState.errors.description)}
              {...form.register('description', { required: 'Description is required' })}
            />
            <Form.Control.Feedback type="invalid">{form.formState.errors.description?.message}</Form.Control.Feedback>
          </Form.Group>
          <Form.Group>
            <Form.Label>Next follow-up date</Form.Label>
            <Form.Control type="date" {...form.register('nextFollowUpDate')} />
          </Form.Group>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="outline-secondary" onClick={onHide}>
            Cancel
          </Button>
          <Button className="btn-gold" type="submit" disabled={addFollowUp.isPending}>
            Save follow-up
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  )
}
