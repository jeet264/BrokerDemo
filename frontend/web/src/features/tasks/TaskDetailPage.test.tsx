import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Route, Routes } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { completeTask, fetchTask } from '../../api/tasks'
import { openTask } from '../../test/fixtures'
import { renderWithProviders } from '../../test/render'
import { TaskDetailPage } from './TaskDetailPage'

vi.mock('../../api/tasks', () => ({
  fetchTask: vi.fn(),
  completeTask: vi.fn(),
  updateTask: vi.fn(),
  reassignTask: vi.fn(),
  cancelTask: vi.fn(),
}))

vi.mock('../../api/clients', () => ({
  fetchClients: vi.fn().mockResolvedValue({ items: [], page: 1, pageSize: 50, totalCount: 0, totalPages: 0 }),
}))

vi.mock('../../api/policies', () => ({
  fetchPolicies: vi.fn().mockResolvedValue({ items: [], page: 1, pageSize: 50, totalCount: 0, totalPages: 0 }),
}))

vi.mock('../../api/renewals', () => ({
  fetchRenewals: vi.fn().mockResolvedValue({ items: [], page: 1, pageSize: 50, totalCount: 0, totalPages: 0 }),
}))

vi.mock('../../api/users', () => ({
  fetchUsers: vi.fn().mockResolvedValue([]),
}))

describe('TaskDetailPage', () => {
  beforeEach(() => {
    vi.mocked(fetchTask).mockResolvedValue(openTask)
    vi.mocked(completeTask).mockResolvedValue({
      ...openTask,
      status: 'Completed',
      completedAtUtc: '2026-08-13T12:00:00Z',
    })
  })

  it('completes a task from the detail page', async () => {
    const user = userEvent.setup()
    renderWithProviders(
      <Routes>
        <Route path="/tasks/:publicId" element={<TaskDetailPage />} />
      </Routes>,
      { route: `/tasks/${openTask.publicId}` },
    )

    expect(await screen.findByRole('heading', { name: openTask.title })).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: 'Complete' }))
    const dialog = await screen.findByRole('dialog')
    expect(dialog).toHaveTextContent('Complete task')
    await user.click(within(dialog).getByRole('button', { name: 'Complete' }))

    await waitFor(() => {
      expect(completeTask).toHaveBeenCalledWith(openTask.publicId)
    })
    expect(await screen.findByText('Call Alpha Logistics was stamped complete.')).toBeInTheDocument()
  })
})
