import { useEffect, useRef, useState } from 'react'
import { useLanguage } from '../../i18n/LanguageProvider'
import { useTheme } from '../../theme/ThemeProvider'
import type { CurrentUser } from '../../types/api'

export function UserProfileDropdown({
  displayName,
  user,
  translatedRole,
  initials,
  onSignOut,
  onOpenProfile,
}: {
  displayName: string
  user: CurrentUser | null
  translatedRole: string
  initials: string
  onSignOut: () => void
  onOpenProfile: () => void
}) {
  const { t } = useLanguage()
  const { theme, toggleTheme } = useTheme()
  const [isOpen, setIsOpen] = useState(false)
  const dropdownRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const handleOutsideClick = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false)
      }
    }
    if (isOpen) {
      document.addEventListener('mousedown', handleOutsideClick)
    }
    return () => {
      document.removeEventListener('mousedown', handleOutsideClick)
    }
  }, [isOpen])

  return (
    <div className="user-profile-dropdown-container" ref={dropdownRef}>
      <button
        type="button"
        className={`user-profile-badge-btn ${isOpen ? 'active' : ''}`}
        onClick={() => setIsOpen((prev) => !prev)}
        aria-expanded={isOpen}
        aria-haspopup="true"
        title={`${displayName} (${user?.email ?? ''})`}
      >
        <div className="user-avatar">{initials}</div>
        <div className="user-badge-info">
          <div className="user-name">{displayName}</div>
          <div className="user-role">{translatedRole}</div>
        </div>
        <i className={`bi bi-chevron-down user-badge-chevron ${isOpen ? 'rotate' : ''}`} />
      </button>

      {isOpen && (
        <div className="user-profile-dropdown-menu">
          <div className="user-profile-menu-header">
            <div className="d-flex align-items-center gap-2">
              <div className="user-avatar-lg">{initials}</div>
              <div className="min-w-0">
                <div className="fw-bold text-dark text-truncate" style={{ fontSize: '0.9rem' }}>
                  {displayName}
                </div>
                <div className="text-muted text-truncate" style={{ fontSize: '0.78rem' }}>
                  {user?.email || 'admin.a@brokeros.test'}
                </div>
                <span className="badge bg-primary-subtle text-primary mt-1" style={{ fontSize: '0.68rem' }}>
                  {translatedRole}
                </span>
              </div>
            </div>
          </div>

          <div className="user-profile-menu-body">
            <button
              type="button"
              className="user-profile-menu-item"
              onClick={() => {
                setIsOpen(false)
                onOpenProfile()
              }}
            >
              <i className="bi bi-person-gear text-primary" />
              <span>{t('chrome.editProfile')}</span>
            </button>

            <button
              type="button"
              className="user-profile-menu-item"
              onClick={() => {
                toggleTheme()
              }}
            >
              <i className={`bi ${theme === 'dark' ? 'bi-sun-fill text-warning' : 'bi-moon-stars-fill text-primary'}`} />
              <span>{theme === 'dark' ? 'Light Mode' : 'Dark Mode'}</span>
            </button>

            <button
              type="button"
              className="user-profile-menu-item text-danger"
              onClick={() => {
                setIsOpen(false)
                onSignOut()
              }}
            >
              <i className="bi bi-box-arrow-right text-danger" />
              <span>{t('chrome.signOut')}</span>
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
