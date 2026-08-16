import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createQuickNote } from '../../api/quickNotes'
import { renderWithProviders } from '../../test/render'
import { QuickNoteModal } from './QuickNoteModal'

vi.mock('../../api/quickNotes', () => ({
  createQuickNote: vi.fn(),
}))

vi.mock('../../api/clients', () => ({
  fetchClients: vi.fn(),
  fetchClient: vi.fn(),
}))

vi.mock('../../api/renewals', () => ({
  fetchRenewals: vi.fn(),
  fetchRenewal: vi.fn(),
}))

describe('QuickNoteModal', () => {
  beforeEach(() => {
    vi.mocked(createQuickNote).mockResolvedValue({
      activityPublicId: 'act-1',
      taskPublicId: null,
      clientPublicId: null,
      clientName: null,
      renewalPublicId: null,
      policyNumber: null,
      text: 'Called, send quote tomorrow.',
      followUpTaskCreated: false,
      createdAtUtc: '2026-08-16T10:00:00Z',
    })
  })

  it('saves a free-text note without extra required fields and stays put', async () => {
    const user = userEvent.setup()
    const onHide = vi.fn()
    renderWithProviders(<QuickNoteModal show onHide={onHide} />)

    expect(screen.getByRole('dialog')).toHaveTextContent('Quick note')
    expect(screen.queryByLabelText('Due date (optional)')).not.toBeInTheDocument()

    await user.type(screen.getByLabelText('Note'), 'Called, send quote tomorrow.')
    await user.click(screen.getByRole('button', { name: 'Save' }))

    await waitFor(() => {
      expect(createQuickNote).toHaveBeenCalledWith({
        text: 'Called, send quote tomorrow.',
        clientPublicId: undefined,
        renewalPublicId: undefined,
        createFollowUpTask: undefined,
        taskDueDateUtc: undefined,
      })
    })
    expect(onHide).toHaveBeenCalled()
    expect(screen.getByText('Note saved')).toBeInTheDocument()
  })

  it('shows an optional due date only when the follow-up task is ticked', async () => {
    const user = userEvent.setup()
    renderWithProviders(<QuickNoteModal show onHide={() => undefined} />)

    await user.click(screen.getByRole('checkbox', { name: 'Also create a follow-up task' }))
    expect(screen.getByLabelText('Due date (optional)')).toBeInTheDocument()
  })
})
