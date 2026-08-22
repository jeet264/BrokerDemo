import { useState } from 'react'
import { Modal } from 'react-bootstrap'
import { useLanguage } from '../../i18n/LanguageProvider'
import { getCurrentUser, setCurrentUser } from '../../api/client'
import type { CurrentUser } from '../../types/api'

export function UserProfileModal({
  show,
  onHide,
  onProfileUpdated,
}: {
  show: boolean
  onHide: () => void
  onProfileUpdated?: () => void
}) {
  const { t } = useLanguage()
  const user: CurrentUser | null = getCurrentUser()

  const [fullName, setFullName] = useState(user?.fullName || 'Apex Admin')
  const [email, setEmail] = useState(user?.email || 'admin.a@brokeros.test')
  const [phone, setPhone] = useState('+91 98200 12345')
  const [organizationName, setOrganizationName] = useState(user?.organizationName || 'Apex Risk Advisors Pvt Ltd')
  const [successMsg, setSuccessMsg] = useState('')
  const [saving, setSaving] = useState(false)

  const handleSave = (e: React.FormEvent) => {
    e.preventDefault()
    setSaving(true)
    setSuccessMsg('')

    setTimeout(() => {
      const updatedUser: CurrentUser = {
        publicUserId: user?.publicUserId || 'usr-demo-admin',
        email: email,
        fullName: fullName,
        role: user?.role || 'BrokerAdmin',
        organizationPublicId: user?.organizationPublicId || 'org-1',
        organizationName: organizationName,
        organizationCode: user?.organizationCode || 'APX',
      }
      setCurrentUser(updatedUser)

      setSaving(false)
      setSuccessMsg('Profile updated successfully!')
      if (onProfileUpdated) {
        onProfileUpdated()
      }
    }, 300)
  }

  return (
    <Modal show={show} onHide={onHide} centered className="user-profile-modal">
      <Modal.Header closeButton>
        <Modal.Title className="h5 mb-0 d-flex align-items-center gap-2">
          <i className="bi bi-person-circle text-primary" aria-hidden />
          {t('chrome.editProfile')}
        </Modal.Title>
      </Modal.Header>
      <form onSubmit={handleSave}>
        <Modal.Body>
          {successMsg && (
            <div className="alert alert-success d-flex align-items-center gap-2 py-2 px-3 mb-3 fs-6">
              <i className="bi bi-check-circle-fill" />
              <span>{successMsg}</span>
            </div>
          )}

          <div className="d-flex align-items-center gap-3 p-3 mb-3 rounded" style={{ background: '#f8fafc', border: '1px solid #e2e8f0' }}>
            <div
              style={{
                width: '48px',
                height: '48px',
                borderRadius: '50%',
                background: '#0d3554',
                color: '#ffffff',
                display: 'grid',
                placeItems: 'center',
                fontSize: '1.2rem',
                fontWeight: 700,
              }}
            >
              {fullName.charAt(0).toUpperCase()}
            </div>
            <div>
              <div className="fw-bold text-dark">{fullName}</div>
              <div className="text-muted small">{email}</div>
              <span className="badge bg-primary-subtle text-primary mt-1" style={{ fontSize: '0.72rem' }}>
                {user?.role === 'BrokerAdmin' ? t('roles.BrokerAdmin') : user?.role === 'BrokerManager' ? t('roles.BrokerManager') : t('roles.BrokerEmployee')}
              </span>
            </div>
          </div>

          <div className="row g-3">
            <div className="col-12">
              <label className="form-label fw-semibold small mb-1">Full Name</label>
              <input
                type="text"
                className="form-control"
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                required
              />
            </div>

            <div className="col-12">
              <label className="form-label fw-semibold small mb-1">Email Address</label>
              <input
                type="email"
                className="form-control"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>

            <div className="col-md-6">
              <label className="form-label fw-semibold small mb-1">Phone / WhatsApp</label>
              <input
                type="text"
                className="form-control"
                value={phone}
                onChange={(e) => setPhone(e.target.value)}
              />
            </div>

            <div className="col-md-6">
              <label className="form-label fw-semibold small mb-1">Organization / Brokerage</label>
              <input
                type="text"
                className="form-control"
                value={organizationName}
                onChange={(e) => setOrganizationName(e.target.value)}
              />
            </div>

            <div className="col-12">
              <label className="form-label fw-semibold small mb-1">System Role (Read-only)</label>
              <input
                type="text"
                className="form-control bg-light"
                value={user?.role === 'BrokerAdmin' ? 'Broker Admin (Full Access)' : user?.role === 'BrokerManager' ? 'Broker Manager' : 'Broker Employee'}
                disabled
              />
            </div>
          </div>
        </Modal.Body>
        <Modal.Footer>
          <button type="button" className="btn btn-outline-secondary" onClick={onHide}>
            Cancel
          </button>
          <button type="submit" className="btn btn-gold" disabled={saving}>
            {saving ? 'Saving...' : 'Save Profile Changes'}
          </button>
        </Modal.Footer>
      </form>
    </Modal>
  )
}
