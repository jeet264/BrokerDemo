import { createBrowserRouter, Navigate } from 'react-router-dom'
import { AppLayout } from '../components/layout/AppLayout'
import { DashboardPage } from '../features/dashboard/DashboardPage'
import { LoginPage } from '../features/auth/LoginPage'
import { PlaceholderPage } from '../features/common/PlaceholderPage'
import { ClientsPage } from '../features/clients/ClientsPage'
import { PoliciesPage } from '../features/policies/PoliciesPage'
import { ClientImportPage, PolicyImportPage } from '../features/import/ImportPages'
import { MyDayPage } from '../features/my-day/MyDayPage'

/**
 * Workspace routes. /login is outside AppLayout.
 * My Day is the default landing after sign-in; the old stats screen lives at /dashboard as Overview.
 */
export const router = createBrowserRouter([
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/',
    element: <AppLayout />,
    children: [
      { index: true, element: <Navigate to="/my-day" replace /> },
      { path: 'my-day', element: <MyDayPage /> },
      { path: 'dashboard', element: <DashboardPage /> },
      { path: 'overview', element: <Navigate to="/dashboard" replace /> },
      { path: 'clients', element: <ClientsPage /> },
      { path: 'clients/import', element: <ClientImportPage /> },
      { path: 'policies', element: <PoliciesPage /> },
      { path: 'policies/import', element: <PolicyImportPage /> },
      { path: 'renewals', element: <PlaceholderPage title="Renewals" description="Upcoming, overdue, and completed renewals will be tracked here." /> },
      { path: 'tasks', element: <PlaceholderPage title="Tasks" description="Follow-ups and renewal work items will be listed here." /> },
      { path: 'activity', element: <PlaceholderPage title="Activity" description="A timeline of brokerage actions will appear here." /> },
      { path: 'insurers', element: <PlaceholderPage title="Insurers" description="The organisation insurer panel will be managed here." /> },
      { path: 'notifications', element: <PlaceholderPage title="Notifications" description="Simulated email and WhatsApp previews will appear here." /> },
      { path: 'team', element: <PlaceholderPage title="Team" description="Broker users for this organisation will be managed here." /> },
    ],
  },
])
