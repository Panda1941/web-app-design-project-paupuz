import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { LogIn, UserPlus } from 'lucide-react'
import { ApiError } from '../api'
import { useAuth } from '../auth'
import { useToast } from '../toast'
import { FieldError, Panel, normalizeErrors } from '../components/common'

export function LoginPage() {
  const { login } = useAuth()
  const toast = useToast()
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  async function submit(e: FormEvent) {
    e.preventDefault()
    setError('')
    setBusy(true)
    try {
      await login(username, password)
      toast.success(`Welcome back, ${username}!`)
      navigate('/')
    } catch (err) {
      setError((err as Error).message)
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="auth-page">
      <Panel title="Log in" icon={<LogIn size={18} />}>
        <form className="form" onSubmit={submit}>
          {error && <div className="form-error">{error}</div>}
          <div className="form-row">
            <label htmlFor="login-username">Username</label>
            <input
              id="login-username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              autoComplete="username"
              required
            />
          </div>
          <div className="form-row">
            <label htmlFor="login-password">Password</label>
            <input
              id="login-password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              autoComplete="current-password"
              required
            />
          </div>
          <button type="submit" className="btn btn-primary btn-block" disabled={busy}>
            {busy ? 'Logging in…' : 'Log in'}
          </button>
          <p className="muted auth-switch">
            No account yet? <Link to="/register">Register here</Link>.
          </p>
          <p className="muted demo-hint">
            Demo accounts: <code>admin</code>, <code>faker_fan</code> (password <code>Password123!</code>)
          </p>
        </form>
      </Panel>
    </div>
  )
}

export function RegisterPage() {
  const { register } = useAuth()
  const toast = useToast()
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirm, setConfirm] = useState('')
  const [errors, setErrors] = useState<Record<string, string[]>>({})
  const [formError, setFormError] = useState('')
  const [busy, setBusy] = useState(false)

  async function submit(e: FormEvent) {
    e.preventDefault()
    setErrors({})
    setFormError('')
    if (password !== confirm) {
      setErrors({ confirm: ['Passwords do not match.'] })
      return
    }
    setBusy(true)
    try {
      await register(username, email, password)
      toast.success('Account created. Welcome!')
      navigate('/')
    } catch (err) {
      if (err instanceof ApiError) {
        setErrors(normalizeErrors(err.fieldErrors))
        setFormError(err.message)
      }
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="auth-page">
      <Panel title="Create an account" icon={<UserPlus size={18} />}>
        <form className="form" onSubmit={submit}>
          {formError && <div className="form-error">{formError}</div>}
          <div className="form-row">
            <label htmlFor="reg-username">Username</label>
            <input
              id="reg-username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              autoComplete="username"
              required
              minLength={3}
              maxLength={30}
              pattern="[A-Za-z0-9_]+"
              title="Letters, numbers and _ only"
            />
            <FieldError errors={errors.username} />
          </div>
          <div className="form-row">
            <label htmlFor="reg-email">Email</label>
            <input
              id="reg-email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              autoComplete="email"
              required
            />
            <FieldError errors={errors.email} />
          </div>
          <div className="form-row-split">
            <div className="form-row">
              <label htmlFor="reg-password">Password</label>
              <input
                id="reg-password"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                autoComplete="new-password"
                required
                minLength={8}
              />
              <FieldError errors={errors.password} />
            </div>
            <div className="form-row">
              <label htmlFor="reg-confirm">Repeat password</label>
              <input
                id="reg-confirm"
                type="password"
                value={confirm}
                onChange={(e) => setConfirm(e.target.value)}
                autoComplete="new-password"
                required
              />
              <FieldError errors={errors.confirm} />
            </div>
          </div>
          <button type="submit" className="btn btn-primary btn-block" disabled={busy}>
            {busy ? 'Creating account…' : 'Register'}
          </button>
          <p className="muted auth-switch">
            Already have an account? <Link to="/login">Log in</Link>.
          </p>
        </form>
      </Panel>
    </div>
  )
}
