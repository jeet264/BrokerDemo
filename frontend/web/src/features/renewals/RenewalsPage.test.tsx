import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { fetchRenewals, markRenewalLost } from '../../api/renewals'
import { emptyPage, openRenewal, renewalListItem } from '../../test/fixtures'
import { renderWithProviders } from '../../test/render'
import { RenewalsPage } from './RenewalsPage'

vi.mock('../../api/renewals', () => ({
  fetchRenewals: vi.fn(),
  completeRenewal: vi.fn(),
  markRenewalLost: vi.fn(),
  createFollowUp: vi.fn(),
}))

describe('RenewalsPage', () => {
  beforeEach(() => {
    vi.mocked(fetchRenewals).mockResolvedValue(emptyPage([renewalListItem]))
    vi.mocked(markRenewalLost).mockResolvedValue({ ...openRenewal, status: 'Lost', currentStage: 'Completed' })
  })

  it('marks a renewal lost from the row menu without opening the detail page', async () => {
    const user = userEvent.setup()
    renderWithProviders(<RenewalsPage />)

    expect(await screen.findByText(renewalListItem.clientName)).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: 'More actions' }))
    await user.click(await screen.findByRole('button', { name: 'Mark Lost' }))

    const dialog = await screen.findByRole('dialog')
    expect(dialog).toHaveTextContent('Mark lost')
    await user.click(within(dialog).getByRole('button', { name: 'Mark lost' }))

    await waitFor(() => {
      expect(markRenewalLost).toHaveBeenCalledWith(renewalListItem.publicId, undefined)
    })
  })
})
