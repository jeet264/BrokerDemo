import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { completeTask, fetchTasks } from '../../api/tasks'
import { fetchUsers } from '../../api/users'
import { emptyPage, openTask, taskListItem } from '../../test/fixtures'
import { renderWithProviders } from '../../test/render'
import { TasksPage } from './TasksPage'

vi.mock('../../api/tasks', () => ({
  fetchTasks: vi.fn(),
  completeTask: vi.fn(),
}))

vi.mock('../../api/users', () => ({
  fetchUsers: vi.fn(),
}))

describe('TasksPage', () => {
  beforeEach(() => {
    vi.mocked(fetchTasks).mockResolvedValue(emptyPage([taskListItem]))
    vi.mocked(fetchUsers).mockResolvedValue([])
    vi.mocked(completeTask).mockResolvedValue({
      ...openTask,
      status: 'Completed',
      completedAtUtc: '2026-08-16T12:00:00Z',
    })
  })

  it('completes a task from the list row', async () => {
    const user = userEvent.setup()
    renderWithProviders(<TasksPage />)

    expect(await screen.findByText(taskListItem.title)).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: 'Mark complete' }))

    await waitFor(() => {
      expect(completeTask).toHaveBeenCalledWith(taskListItem.publicId)
    })
  })
})
