import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { NavLink, Outlet, useLocation, useMatch, useNavigate } from 'react-router-dom'
import { fetchCurrentUser, logout } from '../../api/auth'
import { getCurrentUser } from '../../api/client'
import { isDemoResetUiEnabled } from '../../lib/demoMode'
import { initials, roleLabel } from '../../lib/format'
import { QuickNoteModal } from '../../features/quickNotes/QuickNoteModal'
import { GlobalSearch } from '../../features/search/GlobalSearch'

const navItems = [
  { to: '/dashboard', label: 'Overview', icon: 'bi-speedometer2' },
  { to: '/my-day', label: 'My Day', icon: 'bi-sun' },
  { to: '/clients', label: 'Clients', icon: 'bi-people' },
  { to: '/policies', label: 'Policies', icon: 'bi-file-earmark-text' },
  { to: '/renewals', label: 'Renewals', icon: 'bi-arrow-repeat' },
  { to: '/tasks', label: 'Tasks', icon: 'bi-check2-square' },
  { to: '/notifications', label: 'Notifications', icon: 'bi-chat-dots' },
]

const mobileNavItems = navItems.filter((item) =>
  ['/dashboard', '/my-day', '/renewals', '/clients', '/tasks'].includes(item.to),
)

function deskChrome(pathname: string) {
  if (pathname.startsWith('/my-day')) {
    return { title: 'My Day', kicker: 'Call these next' }
  }
  if (pathname.startsWith('/clients/import')) {
    return { title: 'Import clients', kicker: 'Excel / CSV' }
  }
  if (pathname.startsWith('/clients')) {
    return { title: pathname === '/clients' ? 'Clients' : 'Client file', kicker: 'The book' }
  }
  if (pathname.startsWith('/policies/import')) {
    return { title: 'Import policies', kicker: 'Excel / CSV' }
  }
  if (pathname.startsWith('/policies')) {
    return { title: pathname === '/policies' ? 'Policies' : 'Policy file', kicker: 'Current term' }
  }
  if (pathname.startsWith('/renewals')) {
    return { title: pathname === '/renewals' ? 'Renewals' : 'Renewal file', kicker: 'Never miss an expiry' }
  }
  if (pathname.startsWith('/tasks')) {
    return { title: pathname === '/tasks' ? 'Tasks' : 'Task', kicker: 'Follow-ups' }
  }
  if (pathname.startsWith('/notifications')) {
    return { title: 'Notifications', kicker: 'Preview only' }
  }
  if (pathname.startsWith('/settings')) {
    return { title: 'Settings', kicker: 'Demo workspace' }
  }
  return { title: 'Overview', kicker: 'What is at risk' }
}

export function AppLayout() {
  const navigate = useNavigate()
  const location = useLocation()
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
  const chrome = deskChrome(location.pathname)

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
            <div className="brand-tag">Never miss a renewal</div>
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
            <div className="header-kicker">{chrome.kicker}</div>
            <h1 className="header-title">{chrome.title}</h1>
          </div>
          <GlobalSearch />
          <div className="header-meta">
            <button type="button" className="quick-note-btn" onClick={() => setQuickNoteOpen(true)}>
              <i className="bi bi-plus-lg" /> Quick Note
            </button>
            <span className="demo-chip">Demo</span>
            {isDemoResetUiEnabled && (
              <NavLink to="/settings" className="header-settings text-decoration-none">
                <i className="bi bi-gear" />
                <span>Settings</span>
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
        <nav className="mobile-tabbar" aria-label="Primary">
          {mobileNavItems.map((item) => (
            <NavLink key={item.to} to={item.to} className={({ isActive }) => `mobile-tab${isActive ? ' active' : ''}`}>
              <i className={`bi ${item.icon}`} />
              {item.label}
            </NavLink>
          ))}
        </nav>
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
