import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { LogIn, UserPlus } from 'lucide-react'
import { useAuth } from '../auth'
import { useToast } from '../toast'
import { checkEmail, checkText, useValidation } from '../validation'
import { CharCount, FieldLabel, FieldMessage, FormSummary, Panel, RequiredNote } from '../components/common'

export function LoginPage() {
  const { login } = useAuth()
  const toast = useToast()
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [busy, setBusy] = useState(false)

  const v = useValidation({
    username: username.trim() ? undefined : 'Username is required.',
    password: password ? undefined : 'Password is required.',
  })

  async function submit(e: FormEvent) {
    e.preventDefault()
    if (!v.trySubmit()) return
    setBusy(true)
    try {
      await login(username, password)
      toast.success(`Welcome back, ${username}!`)
      navigate('/')
    } catch (err) {
      v.handleApiError(err)
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="auth-page">
      <Panel title="Log in" icon={<LogIn size={18} />}>
        <form className="form" onSubmit={submit} noValidate>
          <RequiredNote />
          <FormSummary messages={v.summary} serverMessage={v.serverMessage} />
          <div className="form-row">
            <FieldLabel field="username" required>
              Username
            </FieldLabel>
            <input
              id="field-username"
              className={v.invalid('username')}
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              onBlur={() => v.touch('username')}
              autoComplete="username"
            />
            <FieldMessage message={v.errorFor('username')} />
          </div>
          <div className="form-row">
            <FieldLabel field="password" required>
              Password
            </FieldLabel>
            <input
              id="field-password"
              type="password"
              className={v.invalid('password')}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              onBlur={() => v.touch('password')}
              autoComplete="current-password"
            />
            <FieldMessage message={v.errorFor('password')} />
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
  const [busy, setBusy] = useState(false)

  // Same rules as RegisterRequest on the backend, plus "passwords match".
  const v = useValidation({
    username:
      checkText(username, 'Username', 3, 30) ??
      (/^[A-Za-z0-9_]+$/.test(username.trim()) ? undefined : 'Username can only contain letters, numbers and _.'),
    email: checkEmail(email),
    password: checkText(password, 'Password', 8, 100),
    confirm: !confirm ? 'Repeat your password.' : confirm !== password ? 'Passwords do not match.' : undefined,
  })

  async function submit(e: FormEvent) {
    e.preventDefault()
    if (!v.trySubmit()) return
    setBusy(true)
    try {
      await register(username, email, password)
      toast.success('Account created. Welcome!')
      navigate('/')
    } catch (err) {
      v.handleApiError(err)
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="auth-page">
      <Panel title="Create an account" icon={<UserPlus size={18} />}>
        <form className="form" onSubmit={submit} noValidate>
          <RequiredNote />
          <FormSummary messages={v.summary} serverMessage={v.serverMessage} />
          <div className="form-row">
            <FieldLabel field="username" required hint={<CharCount value={username} min={3} max={30} />}>
              Username
            </FieldLabel>
            <input
              id="field-username"
              className={v.invalid('username')}
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              onBlur={() => v.touch('username')}
              autoComplete="username"
              maxLength={30}
              placeholder="letters, numbers and _"
            />
            <FieldMessage message={v.errorFor('username')} />
          </div>
          <div className="form-row">
            <FieldLabel field="email" required>
              Email
            </FieldLabel>
            <input
              id="field-email"
              type="email"
              className={v.invalid('email')}
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              onBlur={() => v.touch('email')}
              autoComplete="email"
              placeholder="name@example.com"
            />
            <FieldMessage message={v.errorFor('email')} />
          </div>
          <div className="form-row-split">
            <div className="form-row">
              <FieldLabel field="password" required hint="min. 8 characters">
                Password
              </FieldLabel>
              <input
                id="field-password"
                type="password"
                className={v.invalid('password')}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                onBlur={() => v.touch('password')}
                autoComplete="new-password"
              />
              <FieldMessage message={v.errorFor('password')} />
            </div>
            <div className="form-row">
              <FieldLabel field="confirm" required>
                Repeat password
              </FieldLabel>
              <input
                id="field-confirm"
                type="password"
                className={v.invalid('confirm')}
                value={confirm}
                onChange={(e) => setConfirm(e.target.value)}
                onBlur={() => v.touch('confirm')}
                autoComplete="new-password"
              />
              <FieldMessage message={v.errorFor('confirm')} />
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
