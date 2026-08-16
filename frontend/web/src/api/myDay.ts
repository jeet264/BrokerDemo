import { getApiData, postApiData } from './client'
import type { MyDayBriefing, MyDayItemKind } from '../types/api'

/** GET /api/my-day — morning briefing for the signed-in user (capped overdue / today / upcoming lists). */
export function fetchMyDay() {
  return getApiData<MyDayBriefing>('/api/my-day')
}

function actionBody(kind: MyDayItemKind, publicId: string) {
  return { kind, publicId }
}

/** POST /api/my-day/complete — finishes a task, or clears a renewal chase without rolling over the policy. */
export function completeMyDayItem(kind: MyDayItemKind, publicId: string) {
  return postApiData<unknown>('/api/my-day/complete', actionBody(kind, publicId))
}

/** POST /api/my-day/call — writes a Call activity. Pair with a tel: link on the client. */
export function logMyDayCall(kind: MyDayItemKind, publicId: string) {
  return postApiData<unknown>('/api/my-day/call', actionBody(kind, publicId))
}

/** POST /api/my-day/follow-up — writes a WhatsApp activity and pushes the next chase two IST days. */
export function sendMyDayFollowUp(kind: MyDayItemKind, publicId: string) {
  return postApiData<unknown>('/api/my-day/follow-up', actionBody(kind, publicId))
}
