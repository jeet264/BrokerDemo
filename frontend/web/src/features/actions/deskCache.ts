import type { QueryClient } from '@tanstack/react-query'
import type { Dashboard, PagedResult, RenewalDetails, RenewalListItem, WorkTaskListItem } from '../../types/api'

/**
 * Query-key prefixes used by desk mutations. Invalidate/patch through these so list, detail,
 * dashboard, and My Day stay in sync without each screen knowing the others' shapes.
 */
export const deskKeys = {
  renewals: ['renewals'] as const,
  renewal: (publicId: string) => ['renewal', publicId] as const,
  tasks: ['tasks'] as const,
  task: (publicId: string) => ['task', publicId] as const,
  dashboard: ['dashboard'] as const,
  myDay: ['my-day'] as const,
  clients: ['clients'] as const,
  policies: ['policies'] as const,
}

export function patchPagedItems<T extends { publicId: string }>(
  queryClient: QueryClient,
  queryKey: readonly unknown[],
  publicId: string,
  updater: (item: T) => T,
) {
  queryClient.setQueriesData<PagedResult<T>>({ queryKey: [...queryKey] }, (current) => {
    if (!current?.items) {
      return current
    }

    let changed = false
    const items = current.items.map((item) => {
      if (item.publicId !== publicId) {
        return item
      }
      changed = true
      return updater(item)
    })
    return changed ? { ...current, items } : current
  })
}

export function applyRenewalDetailsToListItem(item: RenewalListItem, updated: RenewalDetails): RenewalListItem {
  return {
    ...item,
    status: updated.status,
    currentStage: updated.currentStage,
    premium: updated.premium,
    expiryDate: updated.expiryDate,
    nextPolicyPublicId: updated.nextPolicyPublicId,
    nextPolicyNumber: updated.nextPolicyNumber,
    nextPolicyExpiryDate: updated.nextPolicyExpiryDate,
    selectedQuotation: updated.selectedQuotation ?? item.selectedQuotation,
  }
}

export function patchRenewalCaches(queryClient: QueryClient, updated: RenewalDetails) {
  queryClient.setQueryData(deskKeys.renewal(updated.publicId), updated)
  patchPagedItems<RenewalListItem>(queryClient, deskKeys.renewals, updated.publicId, (item) =>
    applyRenewalDetailsToListItem(item, updated),
  )
  queryClient.setQueriesData<Dashboard>({ queryKey: deskKeys.dashboard }, (current) => {
    if (!current) {
      return current
    }
    return {
      ...current,
      upcomingRenewals: current.upcomingRenewals.map((row) =>
        row.renewalPublicId === updated.publicId
          ? {
              ...row,
              status: updated.status,
              premium: updated.premium,
              expiryDate: updated.expiryDate,
            }
          : row,
      ),
    }
  })
}

export function patchTaskListStatus(
  queryClient: QueryClient,
  publicId: string,
  status: string,
  completedAtUtc: string | null,
) {
  patchPagedItems<WorkTaskListItem>(queryClient, deskKeys.tasks, publicId, (item) => ({
    ...item,
    status,
    completedAtUtc,
  }))
  queryClient.setQueriesData<Dashboard>({ queryKey: deskKeys.dashboard }, (current) => {
    if (!current) {
      return current
    }
    return {
      ...current,
      todaysTasks: current.todaysTasks.map((row) =>
        row.publicId === publicId ? { ...row, status } : row,
      ),
    }
  })
}

export function invalidateDesk(queryClient: QueryClient) {
  void queryClient.invalidateQueries({ queryKey: deskKeys.renewals })
  void queryClient.invalidateQueries({ queryKey: deskKeys.tasks })
  void queryClient.invalidateQueries({ queryKey: deskKeys.dashboard })
  void queryClient.invalidateQueries({ queryKey: deskKeys.myDay })
  void queryClient.invalidateQueries({ queryKey: deskKeys.policies })
  void queryClient.invalidateQueries({ queryKey: deskKeys.clients })
}
