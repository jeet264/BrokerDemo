const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

export function formatDateIn(isoDate: string | null | undefined) {
  if (!isoDate) {
    return '—'
  }

  const [year, month, day] = isoDate.slice(0, 10).split('-').map(Number)
  if (!year || !month || !day) {
    return isoDate
  }

  return `${day} ${MONTHS[month - 1]} ${year}`
}

export function formatDateTimeIst(utcIso: string | null | undefined) {
  if (!utcIso) {
    return '—'
  }

  const date = new Date(utcIso)
  if (Number.isNaN(date.getTime())) {
    return utcIso
  }

  return new Intl.DateTimeFormat('en-IN', {
    dateStyle: 'medium',
    timeStyle: 'short',
    timeZone: 'Asia/Kolkata',
  }).format(date)
}

export function humanizeEnum(value: string | null | undefined) {
  if (!value) {
    return '—'
  }

  return value.replace(/([a-z])([A-Z])/g, '$1 $2').replace(/([A-Z]+)([A-Z][a-z])/g, '$1 $2')
}

export function roleLabel(role: string | null | undefined) {
  if (!role) {
    return '—'
  }
  if (role === 'BrokerAdmin') {
    return 'Broker Admin'
  }
  return humanizeEnum(role)
}

export function initials(name: string | null | undefined) {
  const parts = (name ?? '').trim().split(/\s+/).filter(Boolean)
  if (parts.length === 0) {
    return 'B'
  }
  if (parts.length === 1) {
    return parts[0].slice(0, 1).toUpperCase()
  }
  return `${parts[0][0]}${parts[parts.length - 1][0]}`.toUpperCase()
}

export function daysRemainingLabel(daysRemaining: number) {
  if (daysRemaining < 0) {
    const days = Math.abs(daysRemaining)
    return `${days} ${days === 1 ? 'day' : 'days'} overdue`
  }
  if (daysRemaining === 0) {
    return 'Due today'
  }
  return `${daysRemaining} ${daysRemaining === 1 ? 'day' : 'days'} remaining`
}

export function daysRemainingShort(daysRemaining: number) {
  if (daysRemaining < 0) {
    return `${Math.abs(daysRemaining)}d overdue`
  }
  if (daysRemaining === 0) {
    return 'Due today'
  }
  return `${daysRemaining}d`
}

export function urgencyFromDays(daysRemaining: number) {
  if (daysRemaining < 0) {
    return 'overdue' as const
  }
  if (daysRemaining === 0) {
    return 'today' as const
  }
  if (daysRemaining <= 7) {
    return 'week' as const
  }
  if (daysRemaining <= 30) {
    return 'month' as const
  }
  return 'ok' as const
}

export function priorityChipClass(priority: string) {
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

export function statusChipClass(status: string) {
  const key = status.toLowerCase()
  if (['overdue', 'lost', 'cancelled', 'expired', 'critical'].includes(key)) {
    return 'status-chip status-chip-danger'
  }
  if (['completed', 'active', 'renewed', 'selected'].includes(key)) {
    return 'status-chip status-chip-ok'
  }
  if (['inprogress', 'pending', 'quotationpending', 'clientdecisionpending', 'pendingrenewal', 'received'].includes(key)) {
    return 'status-chip status-chip-warn'
  }
  if (['rejected'].includes(key)) {
    return 'status-chip status-chip-neutral'
  }
  return 'status-chip status-chip-neutral'
}

export function telHref(phone: string) {
  return `tel:${phone.replace(/[^\d+]/g, '')}`
}
