import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { NavLink, Outlet, useMatch, useNavigate } from 'react-router-dom'
import { fetchCurrentUser, logout } from '../../api/auth'
import { getCurrentUser } from '../../api/client'
import { isDemoResetUiEnabled } from '../../lib/demoMode'
import { initials, roleLabel } from '../../lib/format'
import { QuickNoteModal } from '../../features/quickNotes/QuickNoteModal'

const navItems = [
  { to: '/dashboard', label: 'Dashboard', icon: 'bi-speedometer2' },
  { to: '/clients', label: 'Clients', icon: 'bi-people' },
  { to: '/policies', label: 'Policies', icon: 'bi-file-earmark-text' },
  { to: '/renewals', label: 'Renewals', icon: 'bi-arrow-repeat' },
  { to: '/tasks', label: 'Tasks', icon: 'bi-check2-square' },
  { to: '/notifications', label: 'Notifications', icon: 'bi-chat-dots' },
]

export function AppLayout() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const storedUser = getCurrentUser()
  const [quickNoteOpen, setQuickNoteOpen] = useState(false)
  const clientPage = useMatch('/clients/:publicId')
  const renewalPage = useMatch('/renewals/:publicId')
  const userQuery = useQuery({
    queryKey: ['me', storedUser?.publicUserId ?? 'anonymous'],
    queryFn: fetchCurrentUser,
    staleTime: 30_000,
    retry: false,
    initialData: storedUser ?? undefined,
  })
  const user = userQuery.data ?? storedUser
  const displayName = user?.fullName ?? 'Broker'
  const organization = user?.organizationName ?? 'Workspace'

  const signOut = () => {
    logout()
    queryClient.clear()
    navigate('/login', { replace: true })
  }

  return (
    <div className="app-shell">
      <aside className="app-sidebar">
        <div className="brand-block">
          <div className="brand-mark">B</div>
          <div>
            <div className="brand-name">BrokerOS</div>
            <div className="brand-tag">Renewal operations</div>
          </div>
        </div>
        <nav className="sidebar-nav" aria-label="Workspace">
          {navItems.map((item) => (
            <NavLink key={item.to} to={item.to} className={({ isActive }) => `sidebar-link${isActive ? ' active' : ''}`}>
              <i className={`bi ${item.icon}`} />
              <span>{item.label}</span>
            </NavLink>
          ))}
        </nav>
        <div className="sidebar-footer">{organization}</div>
      </aside>
      <div className="app-main">
        <header className="app-header">
          <div>
            <div className="header-kicker">{organization}</div>
            <h1 className="header-title">Broker operations</h1>
          </div>
          <div className="header-meta">
            <button type="button" className="quick-note-btn" onClick={() => setQuickNoteOpen(true)}>
              <i className="bi bi-plus-lg" /> Quick Note
            </button>
            <span className="demo-chip">Demo workspace</span>
            {isDemoResetUiEnabled && (
              <NavLink to="/settings" className="header-settings text-decoration-none">
                <i className="bi bi-gear" />
                Settings
              </NavLink>
            )}
            <div className="user-chip">
              <span className="user-avatar">{initials(displayName)}</span>
              <div>
                <div className="user-name">{displayName}</div>
                <div className="user-role">{roleLabel(user?.role)}</div>
              </div>
              <button type="button" className="sign-out-btn" onClick={signOut}>
                Sign out
              </button>
            </div>
          </div>
        </header>
        <main className="app-content">
          <Outlet />
        </main>
      </div>
      <QuickNoteModal
        show={quickNoteOpen}
        onHide={() => setQuickNoteOpen(false)}
        contextClientPublicId={clientPage?.params.publicId}
        contextRenewalPublicId={renewalPage?.params.publicId}
      />
    </div>
  )
}
