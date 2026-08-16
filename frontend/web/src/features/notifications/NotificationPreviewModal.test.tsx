import { screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { renderWithProviders } from '../../test/render'
import { NotificationPreviewModal } from './NotificationPreviewModal'
import { SIMULATION_BADGE } from './notificationDisplay'
import type { OutboundNotification } from '../../types/api'

const whatsappNote: OutboundNotification = {
  publicId: 'note-1',
  renewalPublicId: 'renewal-1',
  clientPublicId: 'client-1',
  clientName: 'Alpha Logistics',
  policyNumber: 'POL-A-NEAR',
  organizationName: 'Apex Insurance Brokers',
  recipientType: 'Client',
  channel: 'WhatsApp',
  recipientName: 'Alpha Logistics',
  recipientAddress: '+91 90000 00001',
  subject: '7 days left on POL-A-NEAR',
  body: 'Hi Alpha Logistics, reminder from Apex Insurance Brokers — POL-A-NEAR expires on 12 Sep 2026 (7 days).',
  status: 'Simulated',
  reminderMilestoneDays: 7,
  createdAtUtc: '2026-08-16T10:00:00Z',
}

const emailNote: OutboundNotification = {
  ...whatsappNote,
  publicId: 'note-2',
  recipientType: 'Insurer',
  channel: 'Email',
  recipientName: 'Test New India',
  recipientAddress: 'uw@newindia.test',
  subject: 'Quotation required — POL-A-NEAR',
  body: 'Dear Test New India,\n\nPlease share a renewal quotation.',
  reminderMilestoneDays: 15,
}

describe('NotificationPreviewModal', () => {
  it('opens a WhatsApp chat bubble by default and keeps the simulation badge', () => {
    renderWithProviders(<NotificationPreviewModal notification={whatsappNote} onHide={() => undefined} />)

    expect(screen.getByText(/WhatsApp preview/)).toBeInTheDocument()
    expect(screen.getAllByText(SIMULATION_BADGE).length).toBeGreaterThan(0)
    expect(screen.getByTestId('whatsapp-preview')).toBeInTheDocument()
    expect(screen.getByText(whatsappNote.body)).toBeInTheDocument()
    expect(screen.queryByText('From')).not.toBeInTheDocument()
  })

  it('keeps email chrome for insurer and internal messages', () => {
    renderWithProviders(<NotificationPreviewModal notification={emailNote} onHide={() => undefined} />)

    expect(screen.getByText(/Email preview/)).toBeInTheDocument()
    expect(screen.getByText(SIMULATION_BADGE)).toBeInTheDocument()
    expect(screen.queryByTestId('whatsapp-preview')).not.toBeInTheDocument()
    expect(screen.getByText('From')).toBeInTheDocument()
    expect(screen.getByText(emailNote.subject)).toBeInTheDocument()
  })
})
