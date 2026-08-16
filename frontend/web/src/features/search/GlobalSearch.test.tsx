import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Route, Routes } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { fetchSearch } from '../../api/search'
import { renderWithProviders } from '../../test/render'
import { GlobalSearch } from './GlobalSearch'
import type { SearchHit } from '../../types/api'

vi.mock('../../api/search', () => ({
  fetchSearch: vi.fn(),
}))

const clientHit: SearchHit = {
  type: 'Client',
  publicId: 'client-1',
  title: 'Alpha Logistics',
  subtitle: '+91 90000 00001 · Mumbai',
  matchedOn: 'Name',
}

const policyHit: SearchHit = {
  type: 'Policy',
  publicId: 'policy-1',
  title: 'POL-A-NEAR',
  subtitle: 'Alpha Logistics · MH-01-AB-4321',
  matchedOn: 'VehicleNumber',
}

function renderSearch() {
  return renderWithProviders(
    <Routes>
      <Route path="/" element={<GlobalSearch />} />
      <Route path="/clients/:publicId" element={<p>Opened client file</p>} />
      <Route path="/policies/:publicId" element={<p>Opened policy file</p>} />
    </Routes>,
  )
}

describe('GlobalSearch', () => {
  beforeEach(() => {
    vi.mocked(fetchSearch).mockResolvedValue({ query: 'Alpha', items: [clientHit, policyHit] })
  })

  it('debounces typing, groups results, and opens a record on click', async () => {
    const user = userEvent.setup()
    renderSearch()

    const box = screen.getByRole('combobox', { name: /Search clients and policies/ })
    await user.type(box, 'Alpha')
    expect(fetchSearch).not.toHaveBeenCalled()

    await waitFor(() => expect(fetchSearch).toHaveBeenCalledWith('Alpha'), { timeout: 1500 })
    expect(fetchSearch).toHaveBeenCalledTimes(1)

    expect(await screen.findByText('Clients')).toBeInTheDocument()
    expect(screen.getByText('Policies')).toBeInTheDocument()
    await user.click(screen.getByRole('option', { name: /^Alpha Logistics/ }))
    expect(screen.getByText('Opened client file')).toBeInTheDocument()
  })

  it('shows a clear empty state and supports keyboard navigation', async () => {
    const user = userEvent.setup()
    vi.mocked(fetchSearch).mockResolvedValue({ query: 'zzzz', items: [] })
    renderSearch()

    const box = screen.getByRole('combobox', { name: /Search clients and policies/ })
    await user.type(box, 'zzzz')
    expect(await screen.findByText("No matches for 'zzzz'")).toBeInTheDocument()

    vi.mocked(fetchSearch).mockResolvedValue({ query: 'POL', items: [policyHit] })
    await user.clear(box)
    await user.type(box, 'POL')
    expect(await screen.findByRole('option', { name: /POL-A-NEAR/ })).toBeInTheDocument()

    await user.keyboard('{ArrowDown}{Enter}')
    expect(screen.getByText('Opened policy file')).toBeInTheDocument()
  })
})
