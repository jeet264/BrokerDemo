import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Route, Routes } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { fetchCurrentUser } from '../../api/auth'
import { setCurrentUser } from '../../api/client'
import { renderWithProviders } from '../../test/render'
import { AppLayout } from './AppLayout'

vi.mock('../../api/auth', () => ({
  fetchCurrentUser: vi.fn(),
  logout: vi.fn(),
}))

vi.mock('../../api/clients', () => ({
  fetchClients: vi.fn(),
  fetchClient: vi.fn(),
}))

vi.mock('../../api/renewals', () => ({
  fetchRenewals: vi.fn(),
  fetchRenewal: vi.fn(),
}))

vi.mock('../../api/quickNotes', () => ({
  createQuickNote: vi.fn(),
}))

vi.mock('../../api/search', () => ({
  fetchSearch: vi.fn(),
}))

const currentUser = {
  publicUserId: 'user-admin',
  email: 'admin.a@brokeros.test',
  fullName: 'Admin A',
  role: 'BrokerAdmin',
  organizationPublicId: 'org-1',
  organizationName: 'Apex Insurance Brokers',
  organizationCode: 'APX',
}

describe('AppLayout quick note', () => {
  beforeEach(() => {
    window.localStorage.clear()
    setCurrentUser(currentUser)
    vi.mocked(fetchCurrentUser).mockResolvedValue(currentUser)
  })

  it('opens Quick Note from the header without leaving the current page', async () => {
    const user = userEvent.setup()
    renderWithProviders(
      <Routes>
        <Route element={<AppLayout />}>
          <Route path="/" element={<p>Still on the desk</p>} />
        </Route>
      </Routes>,
    )

    expect(screen.getByText('Still on the desk')).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: /Quick Note/ }))
    expect(screen.getByRole('dialog')).toHaveTextContent('Quick note')
    expect(screen.getByText('Still on the desk')).toBeInTheDocument()
  })

  it('switches the main tab bar to Hindi and Gujarati', async () => {
    const user = userEvent.setup()
    renderWithProviders(
      <Routes>
        <Route element={<AppLayout />}>
          <Route path="/" element={<p>Desk</p>} />
        </Route>
      </Routes>,
    )

    expect(screen.getAllByText('Overview').length).toBeGreaterThan(0)
    await user.click(screen.getAllByRole('button', { name: 'हिन्दी' })[0])
    expect(screen.getAllByText('अवलोकन').length).toBeGreaterThan(0)
    expect(screen.getAllByText('मेरा दिन').length).toBeGreaterThan(0)
    await user.click(screen.getAllByRole('button', { name: 'ગુજરાતી' })[0])
    expect(screen.getAllByText('અવલોકન').length).toBeGreaterThan(0)
    expect(screen.getAllByText('મારો દિવસ').length).toBeGreaterThan(0)
  })
})
