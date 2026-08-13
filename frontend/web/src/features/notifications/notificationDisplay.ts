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

export { formatDateTimeIst as formatIst } from '../../lib/format'

export function fromAddress(notification: OutboundNotification) {
  const org = notification.organizationName ?? 'BrokerOS'
  return `${org} <renewals@brokeros.demo>`
}
