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

export function formatExpiryLong(isoDate: string) {
  const [year, month, day] = isoDate.split('-').map(Number)
  const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']
  return `${day} ${months[month - 1]} ${year}`
}

export function daysRemainingCopy(daysRemaining: number) {
  if (daysRemaining < 0) {
    const days = Math.abs(daysRemaining)
    return `${days} ${days === 1 ? 'day' : 'days'} overdue`
  }
  if (daysRemaining === 0) {
    return 'Due today'
  }
  return `${daysRemaining} ${daysRemaining === 1 ? 'day' : 'days'} remaining`
}

export function daysShort(daysRemaining: number) {
  if (daysRemaining < 0) {
    return `${Math.abs(daysRemaining)}d overdue`
  }
  if (daysRemaining === 0) {
    return 'Due today'
  }
  return `${daysRemaining}d`
}

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

export function formatIst(utcIso: string) {
  return new Intl.DateTimeFormat('en-IN', {
    dateStyle: 'medium',
    timeStyle: 'short',
    timeZone: 'Asia/Kolkata',
  }).format(new Date(utcIso))
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

export function priorityClass(priority: string) {
  const key = priority.toLowerCase()
  if (key === 'critical') {
    return 'priority-chip priority-chip-critical'
  }
  if (key === 'high') {
    return 'priority-chip priority-chip-high'
  }
  if (key === 'low') {
    return 'priority-chip priority-chip-low'
  }
  return 'priority-chip priority-chip-medium'
}
