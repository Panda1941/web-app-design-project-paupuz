import { useEffect, useState, type ReactNode } from 'react'
import { Link, NavLink, useLocation, useNavigate } from 'react-router-dom'
import { BookOpen, Crown, LogIn, LogOut, Menu, Shield, Swords, UserPlus, Users, X } from 'lucide-react'
import { useAuth } from '../auth'
import { useToast } from '../toast'
import { POSITIONS, positionImage } from '../data/gameData'

export function Layout({ children }: { children: ReactNode }) {
  return (
    <div className="page">
      <Header />
      <div className="container main-grid">
        <main className="content">{children}</main>
        <Sidebar />
      </div>
      <Footer />
    </div>
  )
}

function Header() {
  const { user, logout } = useAuth()
  const toast = useToast()
  const navigate = useNavigate()
  const location = useLocation()
  const [menuOpen, setMenuOpen] = useState(false)

  // Close the mobile menu after navigating.
  useEffect(() => setMenuOpen(false), [location.pathname])

  async function handleLogout() {
    await logout()
    toast.success('You have been logged out.')
    navigate('/')
  }

  return (
    <header className="site-header">
      <div className="container header-inner">
        <Link to="/" className="logo">
          <Swords size={30} className="logo-icon" />
          <span>
            <span className="logo-title">LoL Builds</span>
            <span className="logo-tagline">Champion builds &amp; guides</span>
          </span>
        </Link>

        <button
          className="hamburger"
          onClick={() => setMenuOpen(!menuOpen)}
          aria-label="Toggle menu"
          aria-expanded={menuOpen}
        >
          {menuOpen ? <X size={24} /> : <Menu size={24} />}
        </button>

        <nav className={`nav ${menuOpen ? 'nav-open' : ''}`}>
          <NavLink to="/" end className="nav-link">
            <Users size={16} /> Champions
          </NavLink>
          {user ? (
            <>
              <span className="nav-user">
                {user.role === 'Admin' ? <Crown size={16} /> : <Shield size={16} />}
                {user.username}
              </span>
              <button className="nav-link nav-button" onClick={handleLogout}>
                <LogOut size={16} /> Log out
              </button>
            </>
          ) : (
            <>
              <NavLink to="/login" className="nav-link">
                <LogIn size={16} /> Log in
              </NavLink>
              <NavLink to="/register" className="nav-link">
                <UserPlus size={16} /> Register
              </NavLink>
            </>
          )}
        </nav>
      </div>
      <div className="header-strip">
        <div className="container">Create, share and discuss champion builds for every role.</div>
      </div>
    </header>
  )
}

function Sidebar() {
  const { user } = useAuth()

  return (
    <aside className="sidebar">
      <section className="panel">
        <h3 className="panel-title">
          <Shield size={15} /> Account
        </h3>
        <div className="panel-body">
          {user ? (
            <>
              <p>
                Logged in as <strong>{user.username}</strong>
              </p>
              <p className="muted">
                Role: <span className={`role-badge role-${user.role.toLowerCase()}`}>{user.role}</span>
              </p>
            </>
          ) : (
            <>
              <p>Log in to create builds and join the discussion.</p>
              <div className="button-row">
                <Link to="/login" className="btn btn-primary btn-small">
                  <LogIn size={14} /> Log in
                </Link>
                <Link to="/register" className="btn btn-small">
                  <UserPlus size={14} /> Register
                </Link>
              </div>
            </>
          )}
        </div>
      </section>

      <section className="panel">
        <h3 className="panel-title">
          <BookOpen size={15} /> Positions
        </h3>
        <ul className="panel-body position-list">
          {POSITIONS.map((p) => (
            <li key={p}>
              <img src={positionImage(p)} alt="" width={20} height={20} />
              {p}
            </li>
          ))}
        </ul>
      </section>

      <section className="panel">
        <h3 className="panel-title">
          <Crown size={15} /> How it works
        </h3>
        <ol className="panel-body how-list">
          <li>Pick a champion.</li>
          <li>Browse builds for each position.</li>
          <li>Post your own build with items, spells and runes.</li>
          <li>Comment and help others improve.</li>
        </ol>
      </section>
    </aside>
  )
}

function Footer() {
  return (
    <footer className="site-footer">
      <div className="container footer-inner">
        <div>
          <h4>LoL Builds</h4>
          <p>A university project for the web application design module.</p>
        </div>
        <div>
          <h4>Links</h4>
          <ul>
            <li>
              <Link to="/">Champions</Link>
            </li>
            <li>
              <Link to="/register">Create an account</Link>
            </li>
          </ul>
        </div>
        <div>
          <h4>Disclaimer</h4>
          <p>
            Not endorsed by Riot Games. League of Legends and all related images are trademarks of Riot Games,
            Inc.
          </p>
        </div>
      </div>
      <div className="footer-bottom">© {new Date().getFullYear()} LoL Builds</div>
    </footer>
  )
}
