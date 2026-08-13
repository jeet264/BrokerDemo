import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { fetchClients } from '../../api/clients'
import { fetchInsurers } from '../../api/insurers'
import { createPolicy, fetchPolicies } from '../../api/policies'
import { fetchUsers } from '../../api/users'
import { clientListItem, createdPolicy, emptyPage, insurer, teamUser } from '../../test/fixtures'
import { renderWithProviders } from '../../test/render'
import { PoliciesPage } from './PoliciesPage'

vi.mock('../../api/clients', () => ({
  fetchClients: vi.fn(),
}))

vi.mock('../../api/insurers', () => ({
  fetchInsurers: vi.fn(),
}))

vi.mock('../../api/policies', () => ({
  fetchPolicies: vi.fn(),
  createPolicy: vi.fn(),
}))

vi.mock('../../api/users', () => ({
  fetchUsers: vi.fn(),
}))

describe('PoliciesPage', () => {
  beforeEach(() => {
    vi.mocked(fetchPolicies).mockResolvedValue(emptyPage())
    vi.mocked(fetchClients).mockResolvedValue(emptyPage([clientListItem]))
    vi.mocked(fetchInsurers).mockResolvedValue([insurer])
    vi.mocked(fetchUsers).mockResolvedValue([teamUser])
    vi.mocked(createPolicy).mockResolvedValue(createdPolicy)
  })

  it('creates a policy from the add form', async () => {
    const user = userEvent.setup()
    renderWithProviders(<PoliciesPage />)

    await user.click(await screen.findByRole('button', { name: '+ Add Policy' }))
    expect(await screen.findByRole('dialog')).toHaveTextContent('Add Policy')

    await user.selectOptions(screen.getByLabelText('Client'), clientListItem.publicId)
    await user.type(screen.getByLabelText('Policy number'), 'POL-NEW-1')
    await user.selectOptions(screen.getByLabelText('Insurer'), insurer.publicId)
    await user.clear(screen.getByLabelText('Premium'))
    await user.type(screen.getByLabelText('Premium'), '500000')
    await user.clear(screen.getByLabelText('Sum insured'))
    await user.type(screen.getByLabelText('Sum insured'), '5000000')
    await user.click(screen.getByRole('button', { name: 'Save policy' }))

    await waitFor(() => {
      expect(createPolicy).toHaveBeenCalledWith(
        expect.objectContaining({
          policyNumber: 'POL-NEW-1',
          clientPublicId: clientListItem.publicId,
          insurerPublicId: insurer.publicId,
          premium: 500000,
          sumInsured: 5000000,
        }),
      )
    })
    expect(await screen.findByText(/POL-NEW-1 commission/)).toBeInTheDocument()
  })
})
