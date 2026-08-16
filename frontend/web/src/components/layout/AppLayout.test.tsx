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
})
