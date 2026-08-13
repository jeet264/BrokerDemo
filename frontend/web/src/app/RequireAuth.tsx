import { Navigate, Outlet } from 'react-router-dom'
import { getAccessToken } from '../api/client'

export function RequireAuth() {
  if (!getAccessToken()) {
    return <Navigate to="/login" replace />
  }

  return <Outlet />
}
