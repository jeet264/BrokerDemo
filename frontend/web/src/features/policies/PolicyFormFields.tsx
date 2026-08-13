import { Form } from 'react-bootstrap'
import { useWatch, type Control, type FieldErrors, type UseFormRegister } from 'react-hook-form'
import type { ClientListItem, InsurerListItem, TeamUser } from '../../types/api'
import { commissionAmount, formatInr } from '../../lib/money'

export const POLICY_TYPES = [
  'Property',
  'Marine',
  'Engineering',
  'Liability',
  'Motor',
  'Health',
  'EmployeeBenefits',
  'Other',
] as const

export interface PolicyFormValues {
  policyNumber: string
  clientPublicId: string
  insurerPublicId: string
  policyType: string
  premium: number
  sumInsured: number
  commissionPercentage: number
  startDate: string
  expiryDate: string
  assignedUserPublicId: string
  notes: string
}

export function defaultPolicyFormValues(): PolicyFormValues {
  const today = new Date()
  const start = today.toISOString().slice(0, 10)
  const expiry = new Date(Date.UTC(today.getUTCFullYear() + 1, today.getUTCMonth(), today.getUTCDate()))
    .toISOString()
    .slice(0, 10)
  return {
    policyNumber: '',
    clientPublicId: '',
    insurerPublicId: '',
    policyType: 'Property',
    premium: 0,
    sumInsured: 0,
    commissionPercentage: 10,
    startDate: start,
    expiryDate: expiry,
    assignedUserPublicId: '',
    notes: '',
  }
}

export function PolicyFormFields({
  register,
  control,
  errors,
  clients,
  insurers,
  users,
  policyNumberOptional,
}: {
  register: UseFormRegister<PolicyFormValues>
  control: Control<PolicyFormValues>
  errors: FieldErrors<PolicyFormValues>
  clients: ClientListItem[]
  insurers: InsurerListItem[]
  users: TeamUser[]
  policyNumberOptional?: boolean
}) {
  const premium = useWatch({ control, name: 'premium' })
  const commissionPercentage = useWatch({ control, name: 'commissionPercentage' })
  const startDate = useWatch({ control, name: 'startDate' })
  const preview = commissionAmount(Number(premium) || 0, Number(commissionPercentage) || 0)

  return (
    <>
      <div className="row">
        <div className="col-md-6">
          <Form.Group className="mb-3">
            <Form.Label>Client</Form.Label>
            <Form.Select
              isInvalid={Boolean(errors.clientPublicId)}
              {...register('clientPublicId', { required: 'Client is required' })}
            >
              <option value="">Select client</option>
              {clients.map((client) => (
                <option key={client.publicId} value={client.publicId}>
                  {client.companyName}
                </option>
              ))}
            </Form.Select>
            <Form.Control.Feedback type="invalid">{errors.clientPublicId?.message}</Form.Control.Feedback>
          </Form.Group>
        </div>
        <div className="col-md-6">
          <Form.Group className="mb-3">
            <Form.Label>Policy number</Form.Label>
            <Form.Control
              placeholder={policyNumberOptional ? 'Leave blank to auto-allocate' : undefined}
              isInvalid={Boolean(errors.policyNumber)}
              {...register('policyNumber', {
                required: policyNumberOptional ? false : 'Policy number is required',
                maxLength: { value: 50, message: 'Policy number must be 50 characters or fewer' },
              })}
            />
            <Form.Control.Feedback type="invalid">{errors.policyNumber?.message}</Form.Control.Feedback>
          </Form.Group>
        </div>
      </div>
      <div className="row">
        <div className="col-md-6">
          <Form.Group className="mb-3">
            <Form.Label>Insurer</Form.Label>
            <Form.Select
              isInvalid={Boolean(errors.insurerPublicId)}
              {...register('insurerPublicId', { required: 'Insurer is required' })}
            >
              <option value="">Select insurer</option>
              {insurers.map((insurer) => (
                <option key={insurer.publicId} value={insurer.publicId}>
                  {insurer.name}
                </option>
              ))}
            </Form.Select>
            <Form.Control.Feedback type="invalid">{errors.insurerPublicId?.message}</Form.Control.Feedback>
          </Form.Group>
        </div>
        <div className="col-md-6">
          <Form.Group className="mb-3">
            <Form.Label>Policy type</Form.Label>
            <Form.Select isInvalid={Boolean(errors.policyType)} {...register('policyType', { required: 'Policy type is required' })}>
              {POLICY_TYPES.map((type) => (
                <option key={type} value={type}>
                  {type === 'EmployeeBenefits' ? 'Employee benefits' : type}
                </option>
              ))}
            </Form.Select>
            <Form.Control.Feedback type="invalid">{errors.policyType?.message}</Form.Control.Feedback>
          </Form.Group>
        </div>
      </div>
      <div className="row">
        <div className="col-md-4">
          <Form.Group className="mb-3">
            <Form.Label>Premium</Form.Label>
            <Form.Control
              type="number"
              step="0.01"
              min="0"
              isInvalid={Boolean(errors.premium)}
              {...register('premium', {
                required: 'Premium is required',
                valueAsNumber: true,
                min: { value: 0, message: 'Premium cannot be negative' },
              })}
            />
            <Form.Control.Feedback type="invalid">{errors.premium?.message}</Form.Control.Feedback>
          </Form.Group>
        </div>
        <div className="col-md-4">
          <Form.Group className="mb-3">
            <Form.Label>Commission %</Form.Label>
            <Form.Control
              type="number"
              step="0.0001"
              min="0"
              max="100"
              isInvalid={Boolean(errors.commissionPercentage)}
              {...register('commissionPercentage', {
                required: 'Commission % is required',
                valueAsNumber: true,
                min: { value: 0, message: 'Commission % cannot be negative' },
                max: { value: 100, message: 'Commission % cannot exceed 100' },
              })}
            />
            <Form.Control.Feedback type="invalid">{errors.commissionPercentage?.message}</Form.Control.Feedback>
          </Form.Group>
        </div>
        <div className="col-md-4">
          <Form.Group className="mb-3">
            <Form.Label>Commission amount</Form.Label>
            <div className="form-control bg-light">{formatInr(preview)}</div>
            <div className="form-text">Calculated from premium × commission %. Saved by the API.</div>
          </Form.Group>
        </div>
      </div>
      <Form.Group className="mb-3">
        <Form.Label>Sum insured</Form.Label>
        <Form.Control
          type="number"
          step="0.01"
          min="0"
          isInvalid={Boolean(errors.sumInsured)}
          {...register('sumInsured', {
            required: 'Sum insured is required',
            valueAsNumber: true,
            min: { value: 0, message: 'Sum insured cannot be negative' },
          })}
        />
        <Form.Control.Feedback type="invalid">{errors.sumInsured?.message}</Form.Control.Feedback>
      </Form.Group>
      <div className="row">
        <div className="col-md-6">
          <Form.Group className="mb-3">
            <Form.Label>Start date</Form.Label>
            <Form.Control
              type="date"
              isInvalid={Boolean(errors.startDate)}
              {...register('startDate', { required: 'Start date is required' })}
            />
            <Form.Control.Feedback type="invalid">{errors.startDate?.message}</Form.Control.Feedback>
          </Form.Group>
        </div>
        <div className="col-md-6">
          <Form.Group className="mb-3">
            <Form.Label>Expiry date</Form.Label>
            <Form.Control
              type="date"
              isInvalid={Boolean(errors.expiryDate)}
              {...register('expiryDate', {
                required: 'Expiry date is required',
                validate: (value) =>
                  !startDate || value > startDate || 'Expiry date must be after the start date.',
              })}
            />
            <Form.Control.Feedback type="invalid">{errors.expiryDate?.message}</Form.Control.Feedback>
          </Form.Group>
        </div>
      </div>
      <Form.Group className="mb-3">
        <Form.Label>Assigned employee</Form.Label>
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
          {...register('notes', { maxLength: { value: 2000, message: 'Notes must be 2000 characters or fewer' } })}
        />
        <Form.Control.Feedback type="invalid">{errors.notes?.message}</Form.Control.Feedback>
      </Form.Group>
    </>
  )
}
