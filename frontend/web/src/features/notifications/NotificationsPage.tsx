import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { fetchNotifications } from '../../api/notifications'
import type { OutboundNotification } from '../../types/api'
import { EmptyState, ErrorBanner, LoadingBlock } from '../../components/feedback/PageFeedback'
import { NotificationPreviewModal } from './NotificationPreviewModal'
import { SIMULATION_BADGE, channelLabel, formatIst, recipientTypeLabel } from './notificationDisplay'

export function NotificationsPage() {
  const [preview, setPreview] = useState<OutboundNotification | null>(null)
  const listQuery = useQuery({
    queryKey: ['notifications'],
    queryFn: fetchNotifications,
  })
  const notifications = listQuery.data ?? []

  return (
    <div>
      <div className="page-heading">
        <h2>Notifications</h2>
        <p>Outbound reminders the renewal worker generates at 90/60/45/30/15/7/1-day milestones. Nothing is actually sent.</p>
      </div>

      <section className="content-card">
        <div className="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-3">
          <span className="sim-badge">{SIMULATION_BADGE}</span>
        </div>
        {listQuery.isError && (
          <ErrorBanner>Could not load notifications. Check your connection and try again.</ErrorBanner>
        )}
        {listQuery.isLoading && <LoadingBlock label="Loading notifications…" />}
        {!listQuery.isLoading && notifications.length === 0 && (
          <EmptyState
            icon="bi-chat-dots"
            title="No simulated notifications yet"
            description="They appear when the renewal worker creates 90/60/45/30/15/7/1-day milestone reminders. Nothing is actually sent."
          />
        )}
        {notifications.length > 0 && (
          <div className="table-responsive table-scroll">
            <table className="table align-middle mb-0">
              <thead>
                <tr>
                  <th>Channel</th>
                  <th>To</th>
                  <th>Subject</th>
                  <th>Policy</th>
                  <th>Created</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {notifications.map((notification) => (
                  <tr key={notification.publicId}>
                    <td>
                      <strong>{channelLabel(notification.channel)}</strong>
                      <div className="text-muted small">{recipientTypeLabel(notification.recipientType)}</div>
                    </td>
                    <td>
                      {notification.recipientName}
                      {notification.recipientAddress && (
                        <div className="text-muted small">{notification.recipientAddress}</div>
                      )}
                    </td>
                    <td>{notification.subject}</td>
                    <td>
                      {notification.policyNumber ? (
                        <Link to={`/renewals/${notification.renewalPublicId}`}>{notification.policyNumber}</Link>
                      ) : (
                        '—'
                      )}
                      {notification.clientName && <div className="text-muted small">{notification.clientName}</div>}
                    </td>
                    <td>{formatIst(notification.createdAtUtc)}</td>
                    <td>
                      <button type="button" className="btn btn-sm btn-outline-secondary" onClick={() => setPreview(notification)}>
                        Preview
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <NotificationPreviewModal notification={preview} onHide={() => setPreview(null)} />
    </div>
  )
}
