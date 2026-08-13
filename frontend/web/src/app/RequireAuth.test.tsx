import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { setAccessToken } from '../api/client'
import { RequireAuth } from './RequireAuth'

describe('RequireAuth', () => {
  it('redirects anonymous visitors to login', () => {
    render(
      <MemoryRouter initialEntries={['/dashboard']}>
        <Routes>
          <Route path="/login" element={<div>Sign in to your brokerage</div>} />
          <Route element={<RequireAuth />}>
            <Route path="/dashboard" element={<div>Protected dashboard</div>} />
          </Route>
        </Routes>
      </MemoryRouter>,
    )

    expect(screen.getByText('Sign in to your brokerage')).toBeInTheDocument()
    expect(screen.queryByText('Protected dashboard')).not.toBeInTheDocument()
  })

  it('renders protected routes when a token is present', () => {
    setAccessToken('jwt-token')
    render(
      <MemoryRouter initialEntries={['/dashboard']}>
        <Routes>
          <Route path="/login" element={<div>Sign in to your brokerage</div>} />
          <Route element={<RequireAuth />}>
            <Route path="/dashboard" element={<div>Protected dashboard</div>} />
          </Route>
        </Routes>
      </MemoryRouter>,
    )

    expect(screen.getByText('Protected dashboard')).toBeInTheDocument()
  })
})
