import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { api, getStoredUser, setSessionClearedHandler, type User } from './api'

type AuthState = {
  user: User | null
  isAdmin: boolean
  login: (username: string, password: string) => Promise<void>
  register: (username: string, email: string, password: string) => Promise<void>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthState | null>(null)

// Keeps the logged-in user in React state so every component can read it with useAuth().
export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(getStoredUser())

  // If the API client drops the session (e.g. the refresh token expired), log out here too.
  useEffect(() => {
    setSessionClearedHandler(() => setUser(null))
  }, [])

  const value: AuthState = {
    user,
    isAdmin: user?.role === 'Admin',
    login: async (username, password) => setUser(await api.login(username, password)),
    register: async (username, email, password) => setUser(await api.register(username, email, password)),
    logout: async () => {
      await api.logout()
      setUser(null)
    },
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthState {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth must be used inside AuthProvider')
  return context
}
