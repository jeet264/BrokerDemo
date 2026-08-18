import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { NavLink, Outlet, useLocation, useMatch, useNavigate } from 'react-router-dom'
import { fetchCurrentUser, logout } from '../../api/auth'
import { getCurrentUser } from '../../api/client'
import { isDemoResetUiEnabled } from '../../lib/demoMode'
import { initials, roleLabel } from '../../lib/format'
import { QuickNoteModal } from '../../features/quickNotes/QuickNoteModal'
import { GlobalSearch } from '../../features/search/GlobalSearch'
import { useLanguage } from '../../i18n/LanguageProvider'
import { LanguageSwitcher } from './LanguageSwitcher'

const navItems = [
  { to: '/dashboard', labelKey: 'nav.overview', icon: 'bi-speedometer2' },
  { to: '/my-day', labelKey: 'nav.myDay', icon: 'bi-sun' },
  { to: '/clients', labelKey: 'nav.clients', icon: 'bi-people' },
  { to: '/policies', labelKey: 'nav.policies', icon: 'bi-file-earmark-text' },
  { to: '/renewals', labelKey: 'nav.renewals', icon: 'bi-arrow-repeat' },
  { to: '/tasks', labelKey: 'nav.tasks', icon: 'bi-check2-square' },
  { to: '/notifications', labelKey: 'nav.notifications', icon: 'bi-chat-dots' },
] as const

const mobileNavItems = navItems.filter((item) =>
  ['/dashboard', '/my-day', '/renewals', '/clients', '/tasks'].includes(item.to),
)

function deskChrome(pathname: string, t: (key: string) => string) {
  if (pathname.startsWith('/my-day')) {
    return { title: t('nav.myDay'), kicker: t('chrome.myDayKicker') }
  }
  if (pathname.startsWith('/clients/import')) {
    return { title: t('chrome.importClients'), kicker: t('chrome.excelCsv') }
  }
  if (pathname.startsWith('/clients')) {
    return {
      title: pathname === '/clients' ? t('nav.clients') : t('chrome.clientFile'),
      kicker: t('chrome.clientsKicker'),
    }
  }
  if (pathname.startsWith('/policies/import')) {
    return { title: t('chrome.importPolicies'), kicker: t('chrome.excelCsv') }
  }
  if (pathname.startsWith('/policies')) {
    return {
      title: pathname === '/policies' ? t('nav.policies') : t('chrome.policyFile'),
      kicker: t('chrome.policiesKicker'),
    }
  }
  if (pathname.startsWith('/renewals')) {
    return {
      title: pathname === '/renewals' ? t('nav.renewals') : t('chrome.renewalFile'),
      kicker: t('chrome.renewalsKicker'),
    }
  }
  if (pathname.startsWith('/tasks')) {
    return {
      title: pathname === '/tasks' ? t('nav.tasks') : t('chrome.task'),
      kicker: t('chrome.tasksKicker'),
    }
  }
  if (pathname.startsWith('/notifications')) {
    return { title: t('nav.notifications'), kicker: t('chrome.notificationsKicker') }
  }
  if (pathname.startsWith('/settings')) {
    return { title: t('chrome.settings'), kicker: t('chrome.settingsKicker') }
  }
  return { title: t('nav.overview'), kicker: t('chrome.overviewKicker') }
}

export function AppLayout() {
  const navigate = useNavigate()
  const location = useLocation()
  const queryClient = useQueryClient()
  const { t } = useLanguage()
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
  const chrome = deskChrome(location.pathname, t)

  const translatedRole = () => {
    if (user?.role === 'BrokerAdmin') {
      return t('roles.BrokerAdmin')
    }
    if (user?.role === 'BrokerManager') {
      return t('roles.BrokerManager')
    }
    if (user?.role === 'BrokerEmployee') {
      return t('roles.BrokerEmployee')
    }
    return roleLabel(user?.role)
  }

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
            <div className="brand-tag">{t('brandTag')}</div>
          </div>
        </div>
        <nav className="sidebar-nav" aria-label={t('nav.workspace')}>
          {navItems.map((item) => (
            <NavLink key={item.to} to={item.to} className={({ isActive }) => `sidebar-link${isActive ? ' active' : ''}`}>
              <i className={`bi ${item.icon}`} />
              <span>{t(item.labelKey)}</span>
            </NavLink>
          ))}
        </nav>
        <div className="sidebar-end">
          <LanguageSwitcher variant="sidebar" />
          <div className="sidebar-footer">{organization}</div>
        </div>
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
              <i className="bi bi-plus-lg" /> {t('chrome.quickNote')}
            </button>
            <span className="demo-chip">{t('chrome.demo')}</span>
            {isDemoResetUiEnabled && (
              <NavLink to="/settings" className="header-settings text-decoration-none">
                <i className="bi bi-gear" />
                <span>{t('chrome.settings')}</span>
              </NavLink>
            )}
            <LanguageSwitcher variant="header" />
            <div className="user-chip">
              <span className="user-avatar">{initials(displayName)}</span>
              <div>
                <div className="user-name">{displayName}</div>
                <div className="user-role">{translatedRole()}</div>
              </div>
              <button type="button" className="sign-out-btn" onClick={signOut}>
                {t('chrome.signOut')}
              </button>
            </div>
          </div>
        </header>
        <main className="app-content">
          <Outlet />
        </main>
        <nav className="mobile-tabbar" aria-label={t('nav.workspace')}>
          {mobileNavItems.map((item) => (
            <NavLink key={item.to} to={item.to} className={({ isActive }) => `mobile-tab${isActive ? ' active' : ''}`}>
              <i className={`bi ${item.icon}`} />
              {t(item.labelKey)}
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
