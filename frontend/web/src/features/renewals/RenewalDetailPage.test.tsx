import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Route, Routes } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { fetchRenewal, fetchRenewalNotifications, fetchRenewalTasks, updateRenewalStage } from '../../api/renewals'
import { openRenewal } from '../../test/fixtures'
import { renderWithProviders } from '../../test/render'
import { RenewalDetailPage } from './RenewalDetailPage'

vi.mock('../../api/renewals', () => ({
  fetchRenewal: vi.fn(),
  fetchRenewalNotifications: vi.fn(),
  fetchRenewalTasks: vi.fn(),
  updateRenewalStage: vi.fn(),
  createFollowUp: vi.fn(),
  createRenewalTask: vi.fn(),
  completeRenewal: vi.fn(),
  markRenewalLost: vi.fn(),
}))

describe('RenewalDetailPage', () => {
  beforeEach(() => {
    vi.mocked(fetchRenewal).mockResolvedValue(openRenewal)
    vi.mocked(fetchRenewalNotifications).mockResolvedValue([])
    vi.mocked(fetchRenewalTasks).mockResolvedValue([])
    vi.mocked(updateRenewalStage).mockResolvedValue({
      ...openRenewal,
      currentStage: 'ClientContact',
      status: 'InProgress',
    })
  })

  it('updates the renewal stage', async () => {
    const user = userEvent.setup()
    renderWithProviders(
      <Routes>
        <Route path="/renewals/:publicId" element={<RenewalDetailPage />} />
      </Routes>,
      { route: `/renewals/${openRenewal.publicId}` },
    )

    expect(await screen.findByRole('heading', { name: openRenewal.clientName })).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: 'Change Stage' }))
    const dialog = await screen.findByRole('dialog')
    expect(dialog).toHaveTextContent('Change stage')
    await user.selectOptions(screen.getByLabelText('Stage'), 'ClientContact')
    await user.click(screen.getByRole('button', { name: 'Save stage' }))

    await waitFor(() => {
      expect(updateRenewalStage).toHaveBeenCalledWith(openRenewal.publicId, {
        stage: 'ClientContact',
        notes: undefined,
      })
    })
  })
})
