import { Form } from 'react-bootstrap'
import type { FieldErrors, UseFormRegister } from 'react-hook-form'
import type { TeamUser } from '../../types/api'

export interface ClientFormValues {
  companyName: string
  clientType: string
  industry: string
  email: string
  phone: string
  addressLine1: string
  city: string
  state: string
  postalCode: string
  assignedUserPublicId: string
  notes: string
}

export const defaultClientFormValues: ClientFormValues = {
  companyName: '',
  clientType: 'Corporate',
  industry: '',
  email: '',
  phone: '',
  addressLine1: '',
  city: '',
  state: '',
  postalCode: '',
  assignedUserPublicId: '',
  notes: '',
}

export function ClientFormFields({
  register,
  errors,
  users,
}: {
  register: UseFormRegister<ClientFormValues>
  errors: FieldErrors<ClientFormValues>
  users: TeamUser[]
}) {
  return (
    <>
      <Form.Group className="mb-3">
        <Form.Label>Company name</Form.Label>
        <Form.Control
          isInvalid={Boolean(errors.companyName)}
          {...register('companyName', {
            required: 'Company name is required',
            maxLength: { value: 200, message: 'Company name must be 200 characters or fewer' },
          })}
        />
        <Form.Control.Feedback type="invalid">{errors.companyName?.message}</Form.Control.Feedback>
      </Form.Group>
      <div className="row">
        <div className="col-md-6">
          <Form.Group className="mb-3">
            <Form.Label>Client type</Form.Label>
            <Form.Select
              isInvalid={Boolean(errors.clientType)}
              {...register('clientType', { required: 'Client type is required' })}
            >
              <option value="Corporate">Corporate</option>
              <option value="SME">SME</option>
              <option value="Individual">Individual</option>
            </Form.Select>
            <Form.Control.Feedback type="invalid">{errors.clientType?.message}</Form.Control.Feedback>
          </Form.Group>
        </div>
        <div className="col-md-6">
          <Form.Group className="mb-3">
            <Form.Label>Industry</Form.Label>
            <Form.Control
              isInvalid={Boolean(errors.industry)}
              {...register('industry', {
                maxLength: { value: 100, message: 'Industry must be 100 characters or fewer' },
              })}
            />
            <Form.Control.Feedback type="invalid">{errors.industry?.message}</Form.Control.Feedback>
          </Form.Group>
        </div>
      </div>
      <div className="row">
        <div className="col-md-6">
          <Form.Group className="mb-3">
            <Form.Label>Email</Form.Label>
            <Form.Control
              type="email"
              isInvalid={Boolean(errors.email)}
              {...register('email', {
                required: 'Email is required',
                pattern: { value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: 'Enter a valid email address' },
                maxLength: { value: 256, message: 'Email must be 256 characters or fewer' },
              })}
            />
            <Form.Control.Feedback type="invalid">{errors.email?.message}</Form.Control.Feedback>
          </Form.Group>
        </div>
        <div className="col-md-6">
          <Form.Group className="mb-3">
            <Form.Label>Phone</Form.Label>
            <Form.Control
              isInvalid={Boolean(errors.phone)}
              {...register('phone', {
                required: 'Phone is required',
                minLength: { value: 8, message: 'Enter a valid phone number' },
                maxLength: { value: 30, message: 'Phone must be 30 characters or fewer' },
              })}
            />
            <Form.Control.Feedback type="invalid">{errors.phone?.message}</Form.Control.Feedback>
          </Form.Group>
        </div>
      </div>
      <Form.Group className="mb-3">
        <Form.Label>Address</Form.Label>
        <Form.Control
          isInvalid={Boolean(errors.addressLine1)}
          {...register('addressLine1', {
            required: 'Address is required',
            maxLength: { value: 200, message: 'Address must be 200 characters or fewer' },
          })}
        />
        <Form.Control.Feedback type="invalid">{errors.addressLine1?.message}</Form.Control.Feedback>
      </Form.Group>
      <div className="row">
        <div className="col-md-4">
          <Form.Group className="mb-3">
            <Form.Label>City</Form.Label>
            <Form.Control
              isInvalid={Boolean(errors.city)}
              {...register('city', {
                required: 'City is required',
                maxLength: { value: 100, message: 'City must be 100 characters or fewer' },
              })}
            />
            <Form.Control.Feedback type="invalid">{errors.city?.message}</Form.Control.Feedback>
          </Form.Group>
        </div>
        <div className="col-md-4">
          <Form.Group className="mb-3">
            <Form.Label>State</Form.Label>
            <Form.Control
              isInvalid={Boolean(errors.state)}
              {...register('state', {
                required: 'State is required',
                maxLength: { value: 100, message: 'State must be 100 characters or fewer' },
              })}
            />
            <Form.Control.Feedback type="invalid">{errors.state?.message}</Form.Control.Feedback>
          </Form.Group>
        </div>
        <div className="col-md-4">
          <Form.Group className="mb-3">
            <Form.Label>PIN code</Form.Label>
            <Form.Control
              isInvalid={Boolean(errors.postalCode)}
              {...register('postalCode', {
                required: 'PIN code is required',
                maxLength: { value: 20, message: 'PIN code must be 20 characters or fewer' },
              })}
            />
            <Form.Control.Feedback type="invalid">{errors.postalCode?.message}</Form.Control.Feedback>
          </Form.Group>
        </div>
      </div>
      <Form.Group className="mb-3">
        <Form.Label>Assigned user</Form.Label>
        <Form.Select isInvalid={Boolean(errors.assignedUserPublicId)} {...register('assignedUserPublicId')}>
          <option value="">Unassigned</option>
          {users.map((user) => (
            <option key={user.publicId} value={user.publicId}>
              {user.fullName}
            </option>
          ))}
        </Form.Select>
        <Form.Control.Feedback type="invalid">{errors.assignedUserPublicId?.message}</Form.Control.Feedback>
      </Form.Group>
      <Form.Group>
        <Form.Label>Notes</Form.Label>
        <Form.Control
          as="textarea"
          rows={3}
          isInvalid={Boolean(errors.notes)}
          {...register('notes', {
            maxLength: { value: 2000, message: 'Notes must be 2000 characters or fewer' },
          })}
        />
        <Form.Control.Feedback type="invalid">{errors.notes?.message}</Form.Control.Feedback>
      </Form.Group>
    </>
  )
}
