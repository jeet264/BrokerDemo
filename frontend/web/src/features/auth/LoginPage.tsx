import { useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import { useToast } from '../../components/feedback/ToastProvider'

interface LoginForm {
  email: string
  password: string
}

/**
 * Sign-in screen.
 *
 * TODO: does not call POST /api/auth/login. Submitting shows a toast and navigates to /dashboard
 * so the shell can be demoed. Do not treat a successful navigation as an authenticated session.
 * Default email is the Development seeder admin; the API still requires Demo@12345 via Swagger until this is wired.
 */
export function LoginPage() {
  const navigate = useNavigate()
  const { showToast } = useToast()
  const { register, handleSubmit } = useForm<LoginForm>({
    defaultValues: {
      email: 'admin@apexbrokers.in',
      password: '',
    },
  })

  const onSubmit = () => {
    showToast('Workspace ready', 'Sign-in will be connected in the authentication phase. Opening the workspace now.')
    navigate('/dashboard')
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
        <p className="login-copy">Track expiries, renewals, and follow-ups in one place.</p>
        <form onSubmit={handleSubmit(onSubmit)} className="login-form">
          <label className="form-label" htmlFor="email">
            Work email
          </label>
          <input id="email" className="form-control" type="email" autoComplete="username" {...register('email', { required: true })} />
          <label className="form-label mt-3" htmlFor="password">
            Password
          </label>
          <input id="password" className="form-control" type="password" autoComplete="current-password" {...register('password')} />
          <button className="btn btn-gold w-100 mt-4" type="submit">
            Continue
          </button>
        </form>
      </div>
    </div>
  )
}
