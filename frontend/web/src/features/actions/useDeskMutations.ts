import { useMutation, useQueryClient } from '@tanstack/react-query'
import { completeRenewal, createFollowUp, markRenewalLost } from '../../api/renewals'
import { completeTask } from '../../api/tasks'
import { useToast } from '../../components/feedback/ToastProvider'
import type { RenewalDetails, WorkTaskDetails } from '../../types/api'
import { deskKeys, invalidateDesk, patchRenewalCaches, patchTaskListStatus } from './deskCache'

/**
 * Single source of truth for desk mutations: Mark Renewed, Mark Lost, Add Follow-up, Complete Task.
 *
 * List views, Renewal/Task detail, the dashboard, and My Day must call these hooks instead of
 * posting to the APIs themselves. Bulk actions should extend this module rather than re-implementing
 * the HTTP calls — that is how Prompt 7B rollover stays correct everywhere:
 * `useMarkRenewed` → `PUT /api/renewals/{id}/complete` → `RenewalService.CompleteRenewalAsync`
 * (inserts a new Policy term; the expired term is not mutated).
 *
 * Lists patch the matching row in the React Query cache (and the dashboard work queues) so the UI
 * updates immediately; related queries are invalidated in the background.
 */
export function useMarkRenewed() {
  const queryClient = useQueryClient()
  const { showToast } = useToast()

  return useMutation({
    mutationFn: (input: { publicId: string; newExpiryDate: string; premium: number }) =>
      completeRenewal(input.publicId, { newExpiryDate: input.newExpiryDate, premium: input.premium }),
    onMutate: async (input) => {
      await queryClient.cancelQueries({ queryKey: deskKeys.renewals })
      const previous = queryClient.getQueriesData({ queryKey: deskKeys.renewals })
      queryClient.setQueriesData({ queryKey: deskKeys.renewals }, (current) =>
        patchRenewalStatusInUnknown(current, input.publicId, 'Renewed', 'Completed'),
      )
      return { previous }
    },
    onError: (error: Error, _input, context) => {
      restoreQueries(queryClient, context?.previous)
      showToast('Could not renew', error.message, 'danger')
    },
    onSuccess: (updated: RenewalDetails) => {
      patchRenewalCaches(queryClient, updated)
      showToast(
        'Policy renewed',
        `Next term ${updated.nextPolicyNumber ?? updated.policyNumber} expires ${updated.nextPolicyExpiryDate ?? updated.expiryDate}.`,
        'success',
      )
    },
    onSettled: () => invalidateDesk(queryClient),
  })
}

export function useMarkLost() {
  const queryClient = useQueryClient()
  const { showToast } = useToast()

  return useMutation({
    mutationFn: (input: { publicId: string; reason?: string }) => markRenewalLost(input.publicId, input.reason),
    onMutate: async (input) => {
      await queryClient.cancelQueries({ queryKey: deskKeys.renewals })
      const previous = queryClient.getQueriesData({ queryKey: deskKeys.renewals })
      queryClient.setQueriesData({ queryKey: deskKeys.renewals }, (current) =>
        patchRenewalStatusInUnknown(current, input.publicId, 'Lost', 'Completed'),
      )
      return { previous }
    },
    onError: (error: Error, _input, context) => {
      restoreQueries(queryClient, context?.previous)
      showToast('Could not mark lost', error.message, 'danger')
    },
    onSuccess: (updated: RenewalDetails) => {
      patchRenewalCaches(queryClient, updated)
      showToast('Marked lost', 'The policy was cancelled. No new term was created.', 'info')
    },
    onSettled: () => invalidateDesk(queryClient),
  })
}

export function useAddFollowUp() {
  const queryClient = useQueryClient()
  const { showToast } = useToast()

  return useMutation({
    mutationFn: (input: {
      publicId: string
      activityType: string
      description: string
      nextFollowUpAtUtc?: string
      createTask?: boolean
      taskTitle?: string
    }) =>
      createFollowUp(input.publicId, {
        activityType: input.activityType,
        description: input.description,
        nextFollowUpAtUtc: input.nextFollowUpAtUtc,
        createTask: input.createTask,
        taskTitle: input.taskTitle,
      }),
    onSuccess: (updated: RenewalDetails) => {
      patchRenewalCaches(queryClient, updated)
      showToast('Follow-up logged', 'The timeline has been updated.', 'success')
    },
    onError: (error: Error) => showToast('Could not log follow-up', error.message, 'danger'),
    onSettled: () => invalidateDesk(queryClient),
  })
}

export function useCompleteTask() {
  const queryClient = useQueryClient()
  const { showToast } = useToast()

  return useMutation({
    mutationFn: (publicId: string) => completeTask(publicId),
    onMutate: async (publicId) => {
      await queryClient.cancelQueries({ queryKey: deskKeys.tasks })
      await queryClient.cancelQueries({ queryKey: deskKeys.dashboard })
      const previousTasks = queryClient.getQueriesData({ queryKey: deskKeys.tasks })
      const previousDashboard = queryClient.getQueriesData({ queryKey: deskKeys.dashboard })
      patchTaskListStatus(queryClient, publicId, 'Completed', new Date().toISOString())
      return { previousTasks, previousDashboard }
    },
    onError: (error: Error, _id, context) => {
      restoreQueries(queryClient, context?.previousTasks)
      restoreQueries(queryClient, context?.previousDashboard)
      showToast('Could not complete task', error.message, 'danger')
    },
    onSuccess: (updated: WorkTaskDetails) => {
      queryClient.setQueryData(deskKeys.task(updated.publicId), updated)
      patchTaskListStatus(queryClient, updated.publicId, updated.status, updated.completedAtUtc)
      showToast('Task completed', `${updated.title} was stamped complete.`, 'success')
    },
    onSettled: () => invalidateDesk(queryClient),
  })
}

function restoreQueries(
  queryClient: ReturnType<typeof useQueryClient>,
  previous?: [readonly unknown[], unknown][],
) {
  previous?.forEach(([key, data]) => {
    queryClient.setQueryData(key, data)
  })
}

function patchRenewalStatusInUnknown(
  current: unknown,
  publicId: string,
  status: string,
  currentStage: string,
): unknown {
  if (!current || typeof current !== 'object' || !('items' in current)) {
    return current
  }
  const page = current as { items: Array<{ publicId: string; status: string; currentStage: string }> }
  if (!Array.isArray(page.items)) {
    return current
  }
  return {
    ...page,
    items: page.items.map((item) =>
      item.publicId === publicId ? { ...item, status, currentStage } : item,
    ),
  }
}
