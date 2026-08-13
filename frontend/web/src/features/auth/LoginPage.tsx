import { useQueryClient } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { Navigate, useNavigate } from 'react-router-dom'
import { login } from '../../api/auth'
import { getAccessToken } from '../../api/client'
import { useToast } from '../../components/feedback/ToastProvider'

interface LoginForm {
  email: string
  password: string
}

const DEMO_PASSWORD = 'Demo@12345'

const DEMO_ACCOUNTS = [
  {
    label: 'Admin',
    email: 'admin@apexbrokers.in',
    hint: 'Full brokerage',
  },
  {
    label: 'Manager',
    email: 'manager@apexbrokers.in',
    hint: 'Full book',
  },
  {
    label: 'Employee',
    email: 'employee@apexbrokers.in',
    hint: 'Assigned work only',
  },
] as const

export function LoginPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { showToast } = useToast()
  const { register, handleSubmit, setValue, watch, formState } = useForm<LoginForm>({
    defaultValues: {
      email: '',
      password: '',
    },
  })
  const selectedEmail = watch('email')

  if (getAccessToken()) {
    return <Navigate to="/dashboard" replace />
  }

  const useDemoAccount = (email: string) => {
    setValue('email', email, { shouldValidate: true, shouldDirty: true })
    setValue('password', DEMO_PASSWORD, { shouldValidate: true, shouldDirty: true })
  }

  const onSubmit = async (values: LoginForm) => {
    try {
      await login(values.email.trim(), values.password)
      queryClient.clear()
      showToast('Signed in', 'Workspace is ready.', 'success')
      navigate('/dashboard', { replace: true })
    } catch (error) {
      showToast('Sign-in failed', error instanceof Error ? error.message : 'Check the API and try again.', 'danger')
    }
  }

  return (
    <div className="login-screen">
      <div className="login-panel">
        <div className="brand-block mb-4">
          <div className="brand-mark">B</div>
          <div>
            <div className="brand-name text-white">BrokerOS</div>
            <div className="brand-tag">Insurance broker operations</div>
          </div>
        </div>
        <h1 className="login-title">Sign in to your brokerage</h1>
        <p className="login-copy">Choose a demo role, then continue. Each login is a separate user.</p>

        <div className="demo-account-grid" role="group" aria-label="Demo accounts">
          {DEMO_ACCOUNTS.map((account) => (
            <button
              key={account.email}
              type="button"
              className={`demo-account-btn${selectedEmail === account.email ? ' is-active' : ''}`}
              onClick={() => useDemoAccount(account.email)}
            >
              <strong>{account.label}</strong>
              <span>{account.hint}</span>
            </button>
          ))}
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="login-form">
          <label className="form-label" htmlFor="email">
            Work email
          </label>
          <input
            id="email"
            className="form-control"
            type="email"
            autoComplete="username"
            {...register('email', { required: 'Enter your work email' })}
          />
          {formState.errors.email && <div className="login-field-error">{formState.errors.email.message}</div>}
          <label className="form-label mt-3" htmlFor="password">
            Password
          </label>
          <input
            id="password"
            className="form-control"
            type="password"
            autoComplete="current-password"
            {...register('password', { required: 'Enter your password' })}
          />
          {formState.errors.password && <div className="login-field-error">{formState.errors.password.message}</div>}
          <button className="btn btn-gold w-100 mt-4" type="submit" disabled={formState.isSubmitting}>
            {formState.isSubmitting ? 'Signing in…' : 'Continue'}
          </button>
        </form>
        <p className="login-copy small mt-3 mb-0">Demo password for every role is Demo@12345.</p>
      </div>
    </div>
  )
}
