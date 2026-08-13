import { Link, NavLink, Outlet } from 'react-router-dom'
import { isDemoResetUiEnabled } from '../../lib/demoMode'

const navItems = [
  { to: '/dashboard', label: 'Dashboard', icon: 'bi-speedometer2' },
  { to: '/clients', label: 'Clients', icon: 'bi-people' },
  { to: '/policies', label: 'Policies', icon: 'bi-file-earmark-text' },
  { to: '/renewals', label: 'Renewals', icon: 'bi-arrow-repeat' },
  { to: '/tasks', label: 'Tasks', icon: 'bi-check2-square' },
  { to: '/activity', label: 'Activity', icon: 'bi-clock-history' },
  { to: '/insurers', label: 'Insurers', icon: 'bi-building' },
  { to: '/notifications', label: 'Notifications', icon: 'bi-chat-dots' },
  { to: '/team', label: 'Team', icon: 'bi-person-badge' },
]

export function AppLayout() {
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
        <nav className="sidebar-nav">
          {navItems.map((item) => (
            <NavLink key={item.to} to={item.to} className={({ isActive }) => `sidebar-link${isActive ? ' active' : ''}`}>
              <i className={`bi ${item.icon}`} />
              <span>{item.label}</span>
            </NavLink>
          ))}
        </nav>
        <div className="sidebar-footer">Apex Insurance Brokers</div>
      </aside>
      <div className="app-main">
        <header className="app-header">
          <div>
            <div className="header-kicker">Workspace</div>
            <h1 className="header-title">Broker operations</h1>
          </div>
          <div className="header-meta">
            <span className="status-pill">Demo</span>
            {isDemoResetUiEnabled && (
              <Link to="/settings" className="header-settings text-decoration-none">
                <i className="bi bi-gear" />
                Settings
              </Link>
            )}
            <Link to="/login" className="user-chip text-decoration-none text-reset">
              <span className="user-avatar">A</span>
              <div>
                <div className="user-name">Admin User</div>
                <div className="user-role">Broker Admin</div>
              </div>
            </Link>
          </div>
        </header>
        <main className="app-content">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
