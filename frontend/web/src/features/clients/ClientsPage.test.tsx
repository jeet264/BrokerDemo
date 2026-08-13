import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createClient, fetchClients } from '../../api/clients'
import { fetchUsers } from '../../api/users'
import { renderWithProviders } from '../../test/render'
import { clientListItem, createdClient, emptyPage, teamUser } from '../../test/fixtures'
import { ClientsPage } from './ClientsPage'

vi.mock('../../api/clients', () => ({
  fetchClients: vi.fn(),
  createClient: vi.fn(),
}))

vi.mock('../../api/users', () => ({
  fetchUsers: vi.fn(),
}))

describe('ClientsPage', () => {
  beforeEach(() => {
    vi.mocked(fetchClients).mockResolvedValue(emptyPage([clientListItem]))
    vi.mocked(fetchUsers).mockResolvedValue([teamUser])
    vi.mocked(createClient).mockResolvedValue(createdClient)
  })

  it('creates a client from the add form', async () => {
    const user = userEvent.setup()
    renderWithProviders(<ClientsPage />)

    await user.click(await screen.findByRole('button', { name: '+ Add Client' }))
    const dialog = await screen.findByRole('dialog')
    expect(dialog).toHaveTextContent('Add Client')

    await user.type(screen.getByLabelText('Company name'), 'Harbor Exports')
    await user.type(screen.getByLabelText('Email'), 'ops@harbor.test')
    await user.type(screen.getByLabelText('Phone'), '+91 90000 33333')
    await user.type(screen.getByLabelText('Address'), '12 Dock Road')
    await user.type(screen.getByLabelText('City'), 'Chennai')
    await user.type(screen.getByLabelText('State'), 'Tamil Nadu')
    await user.type(screen.getByLabelText('PIN code'), '600001')
    await user.click(screen.getByRole('button', { name: 'Save client' }))

    await waitFor(() => {
      expect(createClient).toHaveBeenCalledWith(
        expect.objectContaining({
          companyName: 'Harbor Exports',
          email: 'ops@harbor.test',
          city: 'Chennai',
          country: 'India',
        }),
      )
    })
    expect(await screen.findByText('Harbor Exports is in the book.')).toBeInTheDocument()
  })
})
