import type { OutboundNotification } from '../../types/api'

export const SIMULATION_BADGE = 'Demo simulation — not actually sent'

export function channelLabel(channel: string) {
  return channel === 'WhatsApp' ? 'WhatsApp' : channel
}

export function recipientTypeLabel(recipientType: string) {
  switch (recipientType) {
    case 'InternalUser':
      return 'Internal'
    case 'Insurer':
      return 'Insurer'
    default:
      return 'Client'
  }
}

export function milestoneLabel(days: number | null) {
  if (days == null) {
    return null
  }

  return days === 1 ? '1-day reminder' : `${days}-day reminder`
}

export function formatIst(utcIso: string) {
  const date = new Date(utcIso)
  if (Number.isNaN(date.getTime())) {
    return utcIso
  }

  return new Intl.DateTimeFormat('en-IN', {
    timeZone: 'Asia/Kolkata',
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(date)
}

export function fromAddress(notification: OutboundNotification) {
  const org = notification.organizationName ?? 'BrokerOS'
  return `${org} <renewals@brokeros.demo>`
}
