import { useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import { login } from '../../api/auth'
import { useToast } from '../../components/feedback/ToastProvider'

interface LoginForm {
  email: string
  password: string
}

export function LoginPage() {
  const navigate = useNavigate()
  const { showToast } = useToast()
  const { register, handleSubmit, formState } = useForm<LoginForm>({
    defaultValues: {
      email: 'admin@apexbrokers.in',
      password: 'Demo@12345',
    },
  })

  const onSubmit = async (values: LoginForm) => {
    try {
      await login(values.email, values.password)
      showToast('Signed in', 'Workspace is ready.', 'success')
      navigate('/dashboard')
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
        <p className="login-copy">Track expiries, renewals, and follow-ups in one place.</p>
        <form onSubmit={handleSubmit(onSubmit)} className="login-form">
          <label className="form-label" htmlFor="email">
            Work email
          </label>
          <input id="email" className="form-control" type="email" autoComplete="username" {...register('email', { required: true })} />
          <label className="form-label mt-3" htmlFor="password">
            Password
          </label>
          <input id="password" className="form-control" type="password" autoComplete="current-password" {...register('password', { required: true })} />
          <button className="btn btn-gold w-100 mt-4" type="submit" disabled={formState.isSubmitting}>
            {formState.isSubmitting ? 'Signing in…' : 'Continue'}
          </button>
        </form>
      </div>
    </div>
  )
}
