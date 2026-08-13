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

    expect(await screen.findByText('Overdue renewals')).toBeInTheDocument()
    expect(screen.getByText('3')).toBeInTheDocument()
    expect(screen.getByText('Premium at risk')).toBeInTheDocument()
    expect(screen.getByText('₹8,50,000.00')).toBeInTheDocument()
    expect(screen.getByText('Alpha Logistics')).toBeInTheDocument()
    expect(screen.getByText('POL-A-NEAR')).toBeInTheDocument()
    expect(screen.getByText("Today's tasks")).toBeInTheDocument()
    expect(screen.getByText('Call Alpha Logistics')).toBeInTheDocument()
  })
})
