import { createBrowserRouter, Navigate } from 'react-router-dom'
import { AppLayout } from '../components/layout/AppLayout'
import { DashboardPage } from '../features/dashboard/DashboardPage'
import { LoginPage } from '../features/auth/LoginPage'
import { PlaceholderPage } from '../features/common/PlaceholderPage'
import { PoliciesPage } from '../features/policies/PoliciesPage'
import { RenewalsPage } from '../features/renewals/RenewalsPage'

export const router = createBrowserRouter([
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/',
    element: <AppLayout />,
    children: [
      { index: true, element: <Navigate to="/dashboard" replace /> },
      { path: 'dashboard', element: <DashboardPage /> },
      { path: 'clients', element: <PlaceholderPage title="Clients" description="Client records for the brokerage will appear here." /> },
      { path: 'policies', element: <PoliciesPage /> },
      { path: 'renewals', element: <RenewalsPage /> },
      { path: 'tasks', element: <PlaceholderPage title="Tasks" description="Follow-ups and renewal work items will be listed here." /> },
      { path: 'activity', element: <PlaceholderPage title="Activity" description="A timeline of brokerage actions will appear here." /> },
      { path: 'insurers', element: <PlaceholderPage title="Insurers" description="The organisation insurer panel will be managed here." /> },
      { path: 'notifications', element: <PlaceholderPage title="Notifications" description="Simulated email and WhatsApp previews will appear here." /> },
      { path: 'team', element: <PlaceholderPage title="Team" description="Broker users for this organisation will be managed here." /> },
    ],
  },
])
