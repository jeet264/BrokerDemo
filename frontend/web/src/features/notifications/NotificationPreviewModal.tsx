import { Modal } from 'react-bootstrap'
import type { OutboundNotification } from '../../types/api'
import { SIMULATION_BADGE, channelLabel, formatIst, fromAddress, milestoneLabel } from './notificationDisplay'

/**
 * Preview of an outbound reminder. WhatsApp chat is the default view — that is what a broker
 * would see on the client's phone. Email chrome is only used for internal/insurer messages.
 * Always shows "Demo simulation — not actually sent".
 */
export function NotificationPreviewModal({
  notification,
  onHide,
}: {
  notification: OutboundNotification | null
  onHide: () => void
}) {
  const isEmail = notification?.channel === 'Email'

  return (
    <Modal show={Boolean(notification)} onHide={onHide} centered size="lg">
      {notification && (
        <>
          <Modal.Header closeButton>
            <Modal.Title>
              {isEmail ? 'Email preview' : 'WhatsApp preview'}
              <span className="sim-badge ms-2">{SIMULATION_BADGE}</span>
            </Modal.Title>
          </Modal.Header>
          <Modal.Body>
            {isEmail ? <EmailPreview notification={notification} /> : <WhatsAppPreview notification={notification} />}
          </Modal.Body>
        </>
      )}
    </Modal>
  )
}

function EmailPreview({ notification }: { notification: OutboundNotification }) {
  return (
    <div>
      <p className="text-muted small mb-2">
        Internal or insurer messages stay on email. Client reminders use WhatsApp.
        {milestoneLabel(notification.reminderMilestoneDays)
          ? ` · ${milestoneLabel(notification.reminderMilestoneDays)}`
          : ''}
      </p>
      <div className="notification-preview-email">
        <div className="notification-preview-email-chrome">
          <div>
            <span className="text-muted">From</span> {fromAddress(notification)}
          </div>
          <div>
            <span className="text-muted">To</span> {notification.recipientName}
            {notification.recipientAddress ? ` <${notification.recipientAddress}>` : ''}
          </div>
          <div>
            <span className="text-muted">Subject</span> {notification.subject}
          </div>
          <div className="text-muted small mt-1">{formatIst(notification.createdAtUtc)}</div>
        </div>
        <div className="notification-preview-email-body">{notification.body}</div>
      </div>
    </div>
  )
}

function WhatsAppPreview({ notification }: { notification: OutboundNotification }) {
  const channelNote = notification.channel === 'SMS' ? 'SMS (shown as a phone chat)' : channelLabel(notification.channel)

  return (
    <div>
      <p className="text-muted small mb-2">
        To {notification.recipientName}
        {notification.recipientAddress ? ` · ${notification.recipientAddress}` : ''}
        {milestoneLabel(notification.reminderMilestoneDays)
          ? ` · ${milestoneLabel(notification.reminderMilestoneDays)}`
          : ''}
        {` · ${channelNote}`}
      </p>
      <div className="whatsapp-phone" data-testid="whatsapp-preview">
        <div className="whatsapp-phone-header">
          <div className="whatsapp-phone-avatar" aria-hidden>
            {(notification.recipientName ?? 'C').slice(0, 1).toUpperCase()}
          </div>
          <div>
            <div className="whatsapp-phone-name">{notification.recipientName}</div>
            <div className="whatsapp-phone-sub">{notification.organizationName ?? 'BrokerOS'} · WhatsApp</div>
          </div>
        </div>
        <div className="whatsapp-thread">
          <div className="whatsapp-meta">Today</div>
          <div className="whatsapp-bubble">
            <div>{notification.body}</div>
            <div className="whatsapp-time">{formatIst(notification.createdAtUtc)}</div>
          </div>
        </div>
      </div>
    </div>
  )
}
