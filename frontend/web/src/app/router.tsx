import { createBrowserRouter, Navigate } from 'react-router-dom'
import { AppLayout } from '../components/layout/AppLayout'
import { ClientsPage } from '../features/clients/ClientsPage'
import { ClientDetailPage } from '../features/clients/ClientDetailPage'
import { DashboardPage } from '../features/dashboard/DashboardPage'
import { LoginPage } from '../features/auth/LoginPage'
import { PlaceholderPage } from '../features/common/PlaceholderPage'
import { ClientImportPage, PolicyImportPage } from '../features/import/ImportPages'
import { MyDayPage } from '../features/my-day/MyDayPage'
import { PoliciesPage } from '../features/policies/PoliciesPage'
import { PolicyDetailPage } from '../features/policies/PolicyDetailPage'
import { NotificationsPage } from '../features/notifications/NotificationsPage'
import { RenewalDetailPage } from '../features/renewals/RenewalDetailPage'
import { RenewalsPage } from '../features/renewals/RenewalsPage'
import { SettingsPage } from '../features/settings/SettingsPage'
import { TaskDetailPage } from '../features/tasks/TaskDetailPage'
import { TasksPage } from '../features/tasks/TasksPage'
import { RequireAuth } from './RequireAuth'

export const router = createBrowserRouter([
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/',
    element: <RequireAuth />,
    children: [
      {
        element: <AppLayout />,
        children: [
          { index: true, element: <Navigate to="/my-day" replace /> },
          { path: 'my-day', element: <MyDayPage /> },
          { path: 'dashboard', element: <DashboardPage /> },
          { path: 'overview', element: <Navigate to="/dashboard" replace /> },
          { path: 'clients', element: <ClientsPage /> },
          { path: 'clients/import', element: <ClientImportPage /> },
          { path: 'clients/:publicId', element: <ClientDetailPage /> },
          { path: 'policies', element: <PoliciesPage /> },
          { path: 'policies/import', element: <PolicyImportPage /> },
          { path: 'policies/:publicId', element: <PolicyDetailPage /> },
          { path: 'renewals', element: <RenewalsPage /> },
          { path: 'renewals/:publicId', element: <RenewalDetailPage /> },
          { path: 'tasks', element: <TasksPage /> },
          { path: 'tasks/:publicId', element: <TaskDetailPage /> },
          { path: 'activity', element: <PlaceholderPage title="Activity" description="A full brokerage activity feed will be available here." /> },
          { path: 'insurers', element: <PlaceholderPage title="Insurers" description="Organisation insurer records will be managed here." /> },
          { path: 'notifications', element: <NotificationsPage /> },
          { path: 'team', element: <PlaceholderPage title="Team" description="Broker users for this organisation will be managed here." /> },
          { path: 'settings', element: <SettingsPage /> },
        ],
      },
    ],
  },
])
