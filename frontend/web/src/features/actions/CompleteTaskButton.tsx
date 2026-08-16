import { isOpenTask } from '../tasks/taskDisplay'
import { useCompleteTask } from './useDeskMutations'

/**
 * Inline complete control for a task row. Goes through `useCompleteTask` so list, detail, dashboard,
 * and My Day stamp CompletedAtUtc the same way.
 */
export function CompleteTaskButton({
  publicId,
  status,
  busy,
}: {
  publicId: string
  status: string
  busy?: boolean
}) {
  const completeTask = useCompleteTask()

  if (!isOpenTask(status)) {
    return null
  }

  const pending = busy || completeTask.isPending

  return (
    <button
      type="button"
      className="btn btn-sm btn-outline-secondary"
      disabled={pending}
      onClick={() => completeTask.mutate(publicId)}
    >
      <i className="bi bi-check2 me-1" aria-hidden />
      Mark complete
    </button>
  )
}
