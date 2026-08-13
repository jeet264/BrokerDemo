export const OPEN_TASK_STATUSES = ['Pending', 'InProgress', 'Overdue']

export const TASK_STATUSES = ['Pending', 'InProgress', 'Overdue', 'Completed', 'Cancelled'] as const

export const TASK_PRIORITIES = ['Low', 'Medium', 'High', 'Critical'] as const

export function isOpenTask(status: string) {
  return OPEN_TASK_STATUSES.includes(status)
}

export function formatIst(utcIso: string) {
  return new Intl.DateTimeFormat('en-IN', {
    dateStyle: 'medium',
    timeStyle: 'short',
    timeZone: 'Asia/Kolkata',
  }).format(new Date(utcIso))
}

export function toDatetimeLocal(utcIso: string) {
  const date = new Date(utcIso)
  const parts = new Intl.DateTimeFormat('en-GB', {
    timeZone: 'Asia/Kolkata',
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    hourCycle: 'h23',
  }).formatToParts(date)
  const value = (type: string) => parts.find((part) => part.type === type)?.value ?? '00'
  return `${value('year')}-${value('month')}-${value('day')}T${value('hour')}:${value('minute')}`
}

export function datetimeLocalToUtc(value: string) {
  return new Date(`${value}:00+05:30`).toISOString()
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
