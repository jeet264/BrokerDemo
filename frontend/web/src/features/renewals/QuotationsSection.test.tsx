import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  fetchRenewalQuotations,
  selectQuotation,
  shareQuotation,
  shareQuotationComparison,
} from '../../api/quotations'
import { renderWithProviders } from '../../test/render'
import type { OutboundNotification, Quotation } from '../../types/api'
import { QuotationsSection } from './QuotationsSection'

vi.mock('../../api/quotations', () => ({
  fetchRenewalQuotations: vi.fn(),
  createQuotation: vi.fn(),
  updateQuotation: vi.fn(),
  selectQuotation: vi.fn(),
  deleteQuotation: vi.fn(),
  shareQuotation: vi.fn(),
  shareQuotationComparison: vi.fn(),
}))

vi.mock('../../api/insurers', () => ({
  fetchInsurers: vi.fn().mockResolvedValue([]),
}))

const cheaper: Quotation = {
  publicId: 'quote-cheap',
  renewalPublicId: 'renewal-1',
  insurerPublicId: 'insurer-tata',
  insurerName: 'Tata AIG',
  premiumAmount: 850000,
  sumInsured: 10000000,
  coverageSummary: 'Fire + burglary',
  validUntil: '2026-09-20',
  status: 'Received',
  notes: null,
  isLowestPremium: true,
  createdAtUtc: '2026-08-16T10:00:00Z',
  modifiedAtUtc: null,
}

const current: Quotation = {
  ...cheaper,
  publicId: 'quote-current',
  insurerPublicId: 'insurer-1',
  insurerName: 'Test New India',
  premiumAmount: 940000,
  isLowestPremium: false,
}

const shared: OutboundNotification = {
  publicId: 'note-share',
  renewalPublicId: 'renewal-1',
  clientPublicId: 'client-1',
  clientName: 'Alpha Logistics',
  policyNumber: 'POL-A-NEAR',
  organizationName: 'Apex Insurance Brokers',
  recipientType: 'Client',
  channel: 'WhatsApp',
  recipientName: 'Alpha Logistics',
  recipientAddress: '+91 90000 00001',
  subject: 'Renewal quote — POL-A-NEAR',
  body: 'Hi Alpha Logistics, Apex Insurance Brokers here. Renewal quote for POL-A-NEAR: Tata AIG — ₹8,50,000.00',
  status: 'Simulated',
  reminderMilestoneDays: null,
  createdAtUtc: '2026-08-16T10:00:00Z',
}

describe('QuotationsSection', () => {
  beforeEach(() => {
    vi.mocked(fetchRenewalQuotations).mockResolvedValue([cheaper, current])
    vi.mocked(selectQuotation).mockResolvedValue({ ...cheaper, status: 'Selected' })
    vi.mocked(shareQuotation).mockResolvedValue(shared)
    vi.mocked(shareQuotationComparison).mockResolvedValue({
      ...shared,
      publicId: 'note-compare',
      subject: 'Quote comparison — POL-A-NEAR',
      body: 'Hi Alpha Logistics, quotes for POL-A-NEAR:\n1. Tata AIG — ₹8,50,000.00 (lowest)',
    })
  })

  it('highlights the lowest premium and can select a quote', async () => {
    const user = userEvent.setup()
    renderWithProviders(<QuotationsSection renewalPublicId="renewal-1" open />)

    expect(await screen.findByText('Tata AIG')).toBeInTheDocument()
    expect(screen.getAllByText('Lowest').length).toBeGreaterThan(0)

    await user.click(screen.getAllByRole('button', { name: 'Select this one' })[0])
    await waitFor(() => {
      expect(selectQuotation).toHaveBeenCalledWith('quote-cheap', expect.anything())
    })
  })

  it('opens the WhatsApp preview after sharing a quote', async () => {
    const user = userEvent.setup()
    renderWithProviders(<QuotationsSection renewalPublicId="renewal-1" open />)

    expect(await screen.findByText('Test New India')).toBeInTheDocument()
    await user.click(screen.getAllByRole('button', { name: 'Share' })[0])

    expect(await screen.findByTestId('whatsapp-preview')).toBeInTheDocument()
    expect(screen.getByText(/Demo simulation/)).toBeInTheDocument()
    expect(shareQuotation).toHaveBeenCalled()
  })
})
