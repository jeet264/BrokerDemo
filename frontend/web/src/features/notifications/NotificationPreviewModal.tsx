import { Modal } from 'react-bootstrap'
import type { OutboundNotification } from '../../types/api'
import { SIMULATION_BADGE, channelLabel, formatIst, fromAddress, milestoneLabel } from './notificationDisplay'

export function NotificationPreviewModal({
  notification,
  onHide,
}: {
  notification: OutboundNotification | null
  onHide: () => void
}) {
  return (
    <Modal show={Boolean(notification)} onHide={onHide} centered size="lg">
      {notification && (
        <>
          <Modal.Header closeButton>
            <Modal.Title>
              {channelLabel(notification.channel)} preview
              <span className="sim-badge ms-2">{SIMULATION_BADGE}</span>
            </Modal.Title>
          </Modal.Header>
          <Modal.Body>
            {notification.channel === 'Email' && <EmailPreview notification={notification} />}
            {notification.channel === 'WhatsApp' && <WhatsAppPreview notification={notification} />}
            {notification.channel === 'SMS' && <SmsPreview notification={notification} />}
          </Modal.Body>
        </>
      )}
    </Modal>
  )
}

function EmailPreview({ notification }: { notification: OutboundNotification }) {
  return (
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
        <div className="text-muted small mt-1">
          {formatIst(notification.createdAtUtc)}
          {milestoneLabel(notification.reminderMilestoneDays)
            ? ` · ${milestoneLabel(notification.reminderMilestoneDays)}`
            : ''}
        </div>
      </div>
      <div className="notification-preview-email-body">{notification.body}</div>
    </div>
  )
}

function WhatsAppPreview({ notification }: { notification: OutboundNotification }) {
  return (
    <div>
      <p className="text-muted small mb-2">
        To {notification.recipientName}
        {notification.recipientAddress ? ` · ${notification.recipientAddress}` : ''}
        {milestoneLabel(notification.reminderMilestoneDays)
          ? ` · ${milestoneLabel(notification.reminderMilestoneDays)}`
          : ''}
      </p>
      <div className="whatsapp-thread">
        <div className="whatsapp-meta">{notification.organizationName ?? 'BrokerOS'}</div>
        <div className="whatsapp-bubble">
          <div className="whatsapp-subject">{notification.subject}</div>
          <div>{notification.body}</div>
          <div className="whatsapp-time">{formatIst(notification.createdAtUtc)}</div>
        </div>
      </div>
    </div>
  )
}

function SmsPreview({ notification }: { notification: OutboundNotification }) {
  return (
    <div>
      <p className="text-muted small mb-2">
        To {notification.recipientName}
        {notification.recipientAddress ? ` · ${notification.recipientAddress}` : ''}
        {milestoneLabel(notification.reminderMilestoneDays)
          ? ` · ${milestoneLabel(notification.reminderMilestoneDays)}`
          : ''}
      </p>
      <div className="sms-thread">
        <div className="sms-bubble">
          <div>{notification.body}</div>
          <div className="sms-time">{formatIst(notification.createdAtUtc)}</div>
        </div>
      </div>
    </div>
  )
}
