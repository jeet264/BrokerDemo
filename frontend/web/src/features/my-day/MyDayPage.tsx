import { Link } from 'react-router-dom'
import { useToast } from '../../components/feedback/ToastProvider'
import { useMyDay } from './useMyDay'
import type { MyDayActionName, MyDayBriefing, MyDayItem } from '../../types/api'

/**
 * Morning landing page: a short checklist of what to do today, not a stats dashboard.
 * Cards carry client name, policy number, and inline Call / Mark Done / Follow-up.
 * Dates on cards are IST calendar strings from the API (DateOnly), not Date objects.
 */
export function MyDayPage() {
  const { briefing, complete, call, followUp } = useMyDay()
  const { showToast } = useToast()
  const data = briefing.data
  const busy = complete.isPending || call.isPending || followUp.isPending

  async function onCall(item: MyDayItem) {
    try {
      await call.mutateAsync({ kind: item.kind, publicId: item.publicId })
      showToast('Call logged', item.clientName ?? 'Client', 'success')
      if (item.clientPhone) {
        window.location.href = `tel:${item.clientPhone}`
      }
    } catch (error) {
      showToast('Could not log call', error instanceof Error ? error.message : 'Try again.', 'danger')
    }
  }

  async function onDone(item: MyDayItem) {
    try {
      await complete.mutateAsync({ kind: item.kind, publicId: item.publicId })
      showToast('Marked done', item.actionNeeded, 'success')
    } catch (error) {
      showToast('Could not mark done', error instanceof Error ? error.message : 'Try again.', 'danger')
    }
  }

  async function onFollowUp(item: MyDayItem) {
    try {
      await followUp.mutateAsync({ kind: item.kind, publicId: item.publicId })
      showToast('Follow-up sent', 'Next chase is in two days.', 'success')
    } catch (error) {
      showToast('Could not send follow-up', error instanceof Error ? error.message : 'Try again.', 'danger')
    }
  }

  return (
    <div className="my-day">
      <div className="page-heading">
        <div>
          <h2>{greeting(data?.generatedAtUtc)} Here’s your day</h2>
          <p>
            {data
              ? `India time ${formatIstDate(data.businessDate)}. Do these next — overdue first, then today, then the next three days.`
              : 'The next calls and follow-ups, already sorted for you.'}
          </p>
        </div>
      </div>

      {briefing.isLoading && (
        <section className="content-card">
          <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true" />
          Loading today’s work…
        </section>
      )}

      {briefing.isError && (
        <div className="alert alert-danger" role="alert">
          {briefing.error instanceof Error ? briefing.error.message : 'Could not load My Day. Sign in and confirm the API is running.'}
        </div>
      )}

      {data && isCaughtUp(data) && (
        <section className="content-card my-day-caught-up">
          <div className="my-day-caught-up-emoji" aria-hidden="true">
            🎉
          </div>
          <h3>You’re all caught up 🎉</h3>
          <p>No overdue chases, nothing due today, and nothing urgent in the next three days.</p>
        </section>
      )}

      {data && !isCaughtUp(data) && (
        <>
          <MyDaySection
            title="Overdue"
            hint="Most overdue first"
            items={data.overdueItems}
            total={data.overdueTotalCount}
            tone="overdue"
            busy={busy}
            onCall={onCall}
            onDone={onDone}
            onFollowUp={onFollowUp}
          />
          <MyDaySection
            title="Due today"
            hint="Calls and tasks for this IST calendar day"
            items={data.dueTodayItems}
            total={data.dueTodayTotalCount}
            tone="today"
            busy={busy}
            onCall={onCall}
            onDone={onDone}
            onFollowUp={onFollowUp}
          />
          <MyDaySection
            title="Coming up"
            hint="Next 3 days, including 7-day escalations"
            items={data.upcomingUrgentItems}
            total={data.upcomingUrgentTotalCount}
            tone="upcoming"
            busy={busy}
            onCall={onCall}
            onDone={onDone}
            onFollowUp={onFollowUp}
          />
        </>
      )}
    </div>
  )
}

function MyDaySection({
  title,
  hint,
  items,
  total,
  tone,
  busy,
  onCall,
  onDone,
  onFollowUp,
}: {
  title: string
  hint: string
  items: MyDayItem[]
  total: number
  tone: 'overdue' | 'today' | 'upcoming'
  busy: boolean
  onCall: (item: MyDayItem) => void
  onDone: (item: MyDayItem) => void
  onFollowUp: (item: MyDayItem) => void
}) {
  if (total === 0) {
    return null
  }

  return (
    <section className="my-day-section">
      <div className="my-day-section-head">
        <div>
          <h3>
            {title}
            <span className={`my-day-count my-day-count-${tone}`}>{total}</span>
          </h3>
          <p>{hint}</p>
        </div>
        <div className="my-day-view-all">
          <Link to="/renewals">View all renewals</Link>
          <Link to="/tasks">View all tasks</Link>
        </div>
      </div>
      <div className="my-day-stack">
        {items.map((item) => (
          <MyDayCard
            key={`${item.kind}-${item.publicId}`}
            item={item}
            busy={busy}
            onCall={onCall}
            onDone={onDone}
            onFollowUp={onFollowUp}
          />
        ))}
      </div>
      {total > items.length && (
        <p className="my-day-truncated">
          Showing the top {items.length}. Open the full renewals or tasks list for the rest.
        </p>
      )}
    </section>
  )
}

function MyDayCard({
  item,
  busy,
  onCall,
  onDone,
  onFollowUp,
}: {
  item: MyDayItem
  busy: boolean
  onCall: (item: MyDayItem) => void
  onDone: (item: MyDayItem) => void
  onFollowUp: (item: MyDayItem) => void
}) {
  const detailsTo = item.kind === 'Renewal' ? `/renewals?id=${item.publicId}` : `/tasks?id=${item.publicId}`

  return (
    <article className={`my-day-card my-day-card-${item.bucket.toLowerCase()}`}>
      <div className="my-day-card-body">
        <div className="my-day-card-meta">
          <span className="my-day-kind">{item.kind === 'Renewal' ? 'Renewal' : 'Task'}</span>
          {item.priority && <span className={`my-day-priority my-day-priority-${item.priority.toLowerCase()}`}>{item.priority}</span>}
          {item.daysOverdue != null && <span className="my-day-overdue-pill">{item.daysOverdue}d overdue</span>}
        </div>
        <p className="my-day-action">{item.actionNeeded}</p>
        <p className="my-day-context">
          {item.clientName ?? 'Unnamed client'}
          {item.policyNumber ? ` · ${item.policyNumber}` : ''}
          {item.clientPhone ? ` · ${item.clientPhone}` : ''}
        </p>
      </div>
      <div className="my-day-card-actions">
        {hasAction(item, 'Call') && (
          <button type="button" className="btn btn-gold btn-sm" disabled={busy} onClick={() => void onCall(item)}>
            <i className="bi bi-telephone-fill me-1" />
            Call
          </button>
        )}
        {hasAction(item, 'MarkDone') && (
          <button type="button" className="btn btn-complete-task btn-sm" disabled={busy} onClick={() => void onDone(item)}>
            <i className="bi bi-check2 me-1" />
            Mark done
          </button>
        )}
        {hasAction(item, 'SendFollowUp') && (
          <button type="button" className="btn btn-followup btn-sm" disabled={busy} onClick={() => void onFollowUp(item)}>
            <i className="bi bi-send me-1" />
            Send follow-up
          </button>
        )}
        {hasAction(item, 'ViewDetails') && (
          <Link to={detailsTo} className="btn btn-action-view btn-sm">
            <i className="bi bi-arrow-right me-1" />
            View details
          </Link>
        )}
      </div>
    </article>
  )
}

function hasAction(item: MyDayItem, action: MyDayActionName) {
  return item.availableActions.includes(action)
}

function isCaughtUp(data: MyDayBriefing) {
  return data.overdueTotalCount === 0 && data.dueTodayTotalCount === 0 && data.upcomingUrgentTotalCount === 0
}

function greeting(utcIso?: string) {
  if (!utcIso) {
    return ''
  }

  const hour = Number(
    new Intl.DateTimeFormat('en-GB', { hour: 'numeric', hour12: false, timeZone: 'Asia/Kolkata' }).format(new Date(utcIso)),
  )
  if (hour < 12) {
    return 'Good morning.'
  }
  if (hour < 17) {
    return 'Good afternoon.'
  }
  return 'Good evening.'
}

function formatIstDate(isoDate: string) {
  const [year, month, day] = isoDate.split('-').map(Number)
  if (!year || !month || !day) {
    return isoDate
  }
  return new Intl.DateTimeFormat('en-IN', { dateStyle: 'medium', timeZone: 'Asia/Kolkata' }).format(
    new Date(Date.UTC(year, month - 1, day, 6, 30)),
  )
}
