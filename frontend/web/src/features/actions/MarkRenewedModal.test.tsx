import { screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { insurer } from '../../test/fixtures'
import { renderWithProviders } from '../../test/render'
import { MarkRenewedModal } from './MarkRenewedModal'

vi.mock('../../api/renewals', () => ({
  completeRenewal: vi.fn(),
  createFollowUp: vi.fn(),
  markRenewalLost: vi.fn(),
}))

vi.mock('../../api/insurers', () => ({
  fetchInsurers: vi.fn().mockResolvedValue([
    { publicId: 'insurer-1', name: 'Test New India', code: 'TNIA', isActive: true },
    { publicId: 'insurer-tata', name: 'Tata AIG', code: 'TATA', isActive: true },
  ]),
}))

describe('MarkRenewedModal', () => {
  it('keeps the original premium when no quotation is selected', async () => {
    renderWithProviders(
      <MarkRenewedModal
        show
        publicId="renewal-1"
        expiryDate="2026-09-12"
        premium={100000}
        onHide={() => undefined}
      />,
    )

    expect(screen.getByLabelText('Premium')).toHaveValue(100000)
    expect(screen.queryByTestId('prefill-from-quotation')).not.toBeInTheDocument()
    expect(screen.queryByLabelText('Insurer')).not.toBeInTheDocument()
  })

  it('shows the selected-quotation hint and pre-fills premium', async () => {
    renderWithProviders(
      <MarkRenewedModal
        show
        publicId="renewal-1"
        expiryDate="2026-09-12"
        premium={100000}
        selectedQuotation={{
          publicId: 'quote-1',
          insurerPublicId: insurer.publicId,
          insurerName: 'Tata AIG',
          premiumAmount: 850000,
          sumInsured: 10000000,
        }}
        onHide={() => undefined}
      />,
    )

    expect(await screen.findByTestId('prefill-from-quotation')).toHaveTextContent(
      'Pre-filled from selected quotation',
    )
    expect(screen.getByLabelText('Premium')).toHaveValue(850000)
  })
})
