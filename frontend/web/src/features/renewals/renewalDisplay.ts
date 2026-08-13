import { daysRemainingLabel, daysRemainingShort, formatDateIn, formatDateTimeIst, humanizeEnum, priorityChipClass } from '../../lib/format'

export const OPEN_RENEWAL_STATUSES = [
  'Upcoming',
  'InProgress',
  'QuotationPending',
  'ClientDecisionPending',
  'Overdue',
]

export const RENEWAL_STAGES = [
  { id: 'NotStarted', label: 'Not Started' },
  { id: 'ClientContact', label: 'Client Contact' },
  { id: 'QuotationRequested', label: 'Quotation Requested' },
  { id: 'QuotationReceived', label: 'Quotation Received' },
  { id: 'ClientDecision', label: 'Client Decision' },
  { id: 'Completed', label: 'Completed' },
] as const

export const FOLLOW_UP_TYPES = [
  { id: 'Call', label: 'Call' },
  { id: 'Email', label: 'Email' },
  { id: 'WhatsApp', label: 'WhatsApp' },
  { id: 'Meeting', label: 'Meeting' },
  { id: 'Note', label: 'Other' },
] as const

export function isOpenRenewal(status: string) {
  return OPEN_RENEWAL_STATUSES.includes(status)
}

export function stageLabel(stage: string) {
  return RENEWAL_STAGES.find((item) => item.id === stage)?.label ?? stage
}

export const formatExpiryLong = formatDateIn
export const daysRemainingCopy = daysRemainingLabel
export const daysShort = daysRemainingShort
export const formatIst = formatDateTimeIst
export const priorityClass = priorityChipClass

export function activityTitle(activityType: string, description: string) {
  switch (activityType) {
    case 'RenewalCreated':
      return 'Renewal created'
    case 'ClientContact':
      return 'Client contacted'
    case 'Call':
    case 'Email':
    case 'WhatsApp':
    case 'Meeting':
    case 'Note':
    case 'InsurerContact':
      return 'Follow-up'
    case 'TaskCreated':
      return 'Task created'
    case 'PolicyRenewed':
      return 'Policy renewed'
    case 'RenewalLost':
      return 'Renewal lost'
    case 'StatusChanged':
      if (description.includes('QuotationRequested')) {
        return 'Quotation requested'
      }
      if (description.includes('QuotationReceived')) {
        return 'Quotation received'
      }
      if (description.includes('ClientDecision')) {
        return 'Client decision'
      }
      if (description.includes('ClientContact')) {
        return 'Client contacted'
      }
      return 'Status change'
    default:
      return activityType
  }
}

export function tomorrowIsoDate() {
  const date = new Date()
  date.setDate(date.getDate() + 1)
  const pad = (value: number) => String(value).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`
}

export function istDateToUtc(isoDate: string, hour = 9) {
  const pad = (value: number) => String(value).padStart(2, '0')
  return new Date(`${isoDate}T${pad(hour)}:00:00+05:30`).toISOString()
}

export function addDays(isoDate: string, days: number) {
  const [year, month, day] = isoDate.split('-').map(Number)
  const date = new Date(Date.UTC(year, month - 1, day + days))
  return date.toISOString().slice(0, 10)
}

export function addYears(isoDate: string, years: number) {
  const [year, month, day] = isoDate.split('-').map(Number)
  const date = new Date(Date.UTC(year + years, month - 1, day))
  return date.toISOString().slice(0, 10)
}

export type RenewalActionId = 'contact' | 'followUp' | 'task' | 'stage' | 'renew' | 'lost'

export function nextRequiredAction(renewal: {
  status: string
  currentStage: string
  daysRemaining: number
  nextFollowUpAtUtc: string | null
  assignedUserName: string | null
}) {
  const owner = renewal.assignedUserName ?? 'Unassigned'
  const followUp = renewal.nextFollowUpAtUtc
    ? ` Next follow-up ${formatDateTimeIst(renewal.nextFollowUpAtUtc)}.`
    : ''

  if (!isOpenRenewal(renewal.status)) {
    return {
      title: 'No further action',
      detail: `This file is ${humanizeEnum(renewal.status).toLowerCase()}. Review the timeline for the record.`,
      cta: '',
      action: null as RenewalActionId | null,
      owner,
    }
  }

  if (renewal.daysRemaining < 0) {
    return {
      title: 'Contact the client now',
      detail: `This policy is past expiry. Recover the renewal before cover lapses.${followUp}`,
      cta: 'Contact Client',
      action: 'contact' as const,
      owner,
    }
  }

  switch (renewal.currentStage) {
    case 'NotStarted':
      return {
        title: 'Contact the client',
        detail: `Start this renewal. Confirm intent and the timeline before expiry.${followUp}`,
        cta: 'Contact Client',
        action: 'contact' as const,
        owner,
      }
    case 'ClientContact':
      return {
        title: 'Request a quotation',
        detail: `The client has been contacted. Move the file to quotation requested and chase the insurer.${followUp}`,
        cta: 'Change Stage',
        action: 'stage' as const,
        owner,
      }
    case 'QuotationRequested':
      return {
        title: 'Chase the insurer',
        detail: `A quotation is outstanding. Follow up with the insurer and record what happened.${followUp}`,
        cta: 'Add Follow-up',
        action: 'followUp' as const,
        owner,
      }
    case 'QuotationReceived':
      return {
        title: 'Get the client decision',
        detail: `Present the quote and record the client's decision.${followUp}`,
        cta: 'Change Stage',
        action: 'stage' as const,
        owner,
      }
    case 'ClientDecision':
      return {
        title: 'Close the renewal',
        detail: `The client has decided. Mark the policy renewed to roll the term, or mark it lost.${followUp}`,
        cta: 'Mark Renewed',
        action: 'renew' as const,
        owner,
      }
    default:
      return {
        title: 'Continue this file',
        detail: `Log the next follow-up so the desk stays current.${followUp}`,
        cta: 'Add Follow-up',
        action: 'followUp' as const,
        owner,
      }
  }
}
