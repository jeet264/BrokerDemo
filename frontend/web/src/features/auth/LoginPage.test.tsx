import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { login } from '../../api/auth'
import { ToastProvider } from '../../components/feedback/ToastProvider'
import { LanguageProvider } from '../../i18n/LanguageProvider'
import { LoginPage } from './LoginPage'

vi.mock('../../api/auth', () => ({
  login: vi.fn(),
}))

function renderLogin() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  })
  return render(
    <QueryClientProvider client={queryClient}>
      <LanguageProvider>
        <ToastProvider>
          <MemoryRouter initialEntries={['/login']}>
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route path="/dashboard" element={<div>Dashboard workspace</div>} />
            </Routes>
          </MemoryRouter>
        </ToastProvider>
      </LanguageProvider>
    </QueryClientProvider>,
  )
}

describe('LoginPage', () => {
  beforeEach(() => {
    vi.mocked(login).mockReset()
    window.localStorage.clear()
  })

  it('starts with Admin selected so Continue works immediately', () => {
    renderLogin()
    expect(screen.getByLabelText('Work email')).toHaveValue('admin@apexbrokers.in')
    expect(screen.getByLabelText('Password')).toHaveValue('Demo@12345')
    expect(screen.getByRole('button', { name: /Admin/ })).toHaveClass('is-active')
  })

  it('signs in as Admin and opens the dashboard', async () => {
    const user = userEvent.setup()
    vi.mocked(login).mockResolvedValue({
      accessToken: 'jwt-token',
      expiresAtUtc: '2026-08-13T20:00:00Z',
      user: {
        publicUserId: 'user-1',
        email: 'admin@apexbrokers.in',
        fullName: 'Admin User',
        role: 'BrokerAdmin',
        organizationPublicId: 'org-1',
        organizationName: 'Apex',
        organizationCode: 'APEX',
      },
    })
    renderLogin()

    await user.click(screen.getByRole('button', { name: /Admin/ }))
    await user.click(screen.getByRole('button', { name: 'Continue' }))

    expect(login).toHaveBeenCalledWith('admin@apexbrokers.in', 'Demo@12345')
    expect(await screen.findByText('Dashboard workspace')).toBeInTheDocument()
    expect(await screen.findByText('Workspace is ready.')).toBeInTheDocument()
  })

  it('fills a separate manager account from the demo picker', async () => {
    const user = userEvent.setup()
    renderLogin()

    await user.click(screen.getByRole('button', { name: /Manager/ }))

    expect(screen.getByLabelText('Work email')).toHaveValue('manager@apexbrokers.in')
    expect(screen.getByLabelText('Password')).toHaveValue('Demo@12345')
  })

  it('shows an error when sign-in fails', async () => {
    const user = userEvent.setup()
    vi.mocked(login).mockRejectedValue(new Error('Invalid email or password.'))
    renderLogin()

    await user.click(screen.getByRole('button', { name: /Employee/ }))
    await user.click(screen.getByRole('button', { name: 'Continue' }))

    expect(login).toHaveBeenCalledWith('employee@apexbrokers.in', 'Demo@12345')
    expect(await screen.findByText('Invalid email or password.')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Sign in to your brokerage' })).toBeInTheDocument()
  })
})
