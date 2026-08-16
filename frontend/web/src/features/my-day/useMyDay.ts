import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { completeMyDayItem, fetchMyDay, logMyDayCall, sendMyDayFollowUp } from '../../api/myDay'
import type { MyDayItemKind } from '../../types/api'

/**
 * Loads GET /api/my-day and the three inline actions.
 * Use on the My Day landing page. After Call / Mark Done / Follow-up the briefing is refetched.
 */
export function useMyDay() {
  const queryClient = useQueryClient()
  const briefing = useQuery({
    queryKey: ['my-day'],
    queryFn: fetchMyDay,
  })

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['my-day'] })

  const complete = useMutation({
    mutationFn: ({ kind, publicId }: { kind: MyDayItemKind; publicId: string }) => completeMyDayItem(kind, publicId),
    onSuccess: invalidate,
  })
  const call = useMutation({
    mutationFn: ({ kind, publicId }: { kind: MyDayItemKind; publicId: string }) => logMyDayCall(kind, publicId),
    onSuccess: invalidate,
  })
  const followUp = useMutation({
    mutationFn: ({ kind, publicId }: { kind: MyDayItemKind; publicId: string }) => sendMyDayFollowUp(kind, publicId),
    onSuccess: invalidate,
  })

  return { briefing, complete, call, followUp }
}
