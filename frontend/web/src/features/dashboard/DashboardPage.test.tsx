import { screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { fetchDashboard } from '../../api/dashboard'
import { dashboard } from '../../test/fixtures'
import { renderWithProviders } from '../../test/render'
import { DashboardPage } from './DashboardPage'

vi.mock('../../api/dashboard', () => ({
  fetchDashboard: vi.fn(),
}))

vi.mock('../../api/renewals', () => ({
  createFollowUp: vi.fn(),
}))

describe('DashboardPage', () => {
  beforeEach(() => {
    vi.mocked(fetchDashboard).mockResolvedValue(dashboard)
  })

  it('renders overdue, premium at risk, and upcoming work', async () => {
    renderWithProviders(<DashboardPage />)

    expect(await screen.findByText('₹8,50,000.00')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: /Admin A/ })).toBeInTheDocument()
    expect(screen.getByText('Overdue renewals')).toBeInTheDocument()
    const overdueCard = screen.getByText('Overdue renewals').closest('article')
    expect(overdueCard).toHaveTextContent('3')
    expect(screen.getByText('Premium at risk')).toBeInTheDocument()
    expect(screen.getAllByText('Alpha Logistics').length).toBeGreaterThan(0)
    expect(screen.getAllByText('POL-A-NEAR').length).toBeGreaterThan(0)
    expect(screen.getByText("Today's tasks")).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Call Alpha Logistics' })).toBeInTheDocument()
  })
})
