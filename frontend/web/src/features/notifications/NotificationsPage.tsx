import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { fetchNotifications } from '../../api/notifications'
import type { OutboundNotification } from '../../types/api'
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
          <div className="alert alert-danger">Could not load notifications. Sign in and confirm the API is running.</div>
        )}
        {listQuery.isLoading && <p className="text-muted mb-0">Loading notifications…</p>}
        {!listQuery.isLoading && notifications.length === 0 && (
          <p className="text-muted mb-0">
            No simulated notifications yet. They appear when the renewal worker creates milestone reminders.
          </p>
        )}
        {notifications.length > 0 && (
          <div className="table-responsive">
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
