import { createBrowserRouter, Navigate } from 'react-router-dom'
import { AppLayout } from '../components/layout/AppLayout'
import { ClientsPage } from '../features/clients/ClientsPage'
import { ClientDetailPage } from '../features/clients/ClientDetailPage'
import { DashboardPage } from '../features/dashboard/DashboardPage'
import { LoginPage } from '../features/auth/LoginPage'
import { PlaceholderPage } from '../features/common/PlaceholderPage'
import { PoliciesPage } from '../features/policies/PoliciesPage'
import { PolicyDetailPage } from '../features/policies/PolicyDetailPage'
import { NotificationsPage } from '../features/notifications/NotificationsPage'
import { RenewalDetailPage } from '../features/renewals/RenewalDetailPage'
import { RenewalsPage } from '../features/renewals/RenewalsPage'
import { TaskDetailPage } from '../features/tasks/TaskDetailPage'
import { TasksPage } from '../features/tasks/TasksPage'

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
      { path: 'clients', element: <ClientsPage /> },
      { path: 'clients/:publicId', element: <ClientDetailPage /> },
      { path: 'policies', element: <PoliciesPage /> },
      { path: 'policies/:publicId', element: <PolicyDetailPage /> },
      { path: 'renewals', element: <RenewalsPage /> },
      { path: 'renewals/:publicId', element: <RenewalDetailPage /> },
      { path: 'tasks', element: <TasksPage /> },
      { path: 'tasks/:publicId', element: <TaskDetailPage /> },
      { path: 'activity', element: <PlaceholderPage title="Activity" description="A timeline of brokerage actions will appear here." /> },
      { path: 'insurers', element: <PlaceholderPage title="Insurers" description="The organisation insurer panel will be managed here." /> },
      { path: 'notifications', element: <NotificationsPage /> },
      { path: 'team', element: <PlaceholderPage title="Team" description="Broker users for this organisation will be managed here." /> },
    ],
  },
])
