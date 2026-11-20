// Everything that talks to the REST API lives here.

export const API_URL: string = import.meta.env.VITE_API_URL ?? 'http://localhost:5224'

// ---------- Types (match the backend DTOs) ----------

export type Link = { href: string; rel: string; method: string }

export type Paged<T> = {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  links: Link[]
}

export type Champion = {
  id: number
  name: string
  title: string
  description: string
  imageUrl: string
  createdAt: string
  links: Link[]
}

export type ChampionInput = Pick<Champion, 'name' | 'title' | 'description' | 'imageUrl'>

export type Build = {
  id: number
  championId: number
  championName: string
  title: string
  description: string
  position: string
  items: number[]
  summonerSpells: string[]
  primaryRuneTree: string
  keystone: string
  secondaryRuneTree: string
  authorId: number
  authorUsername: string
  commentCount: number
  createdAt: string
  updatedAt: string
  links: Link[]
}

export type BuildInput = Pick<
  Build,
  | 'title'
  | 'description'
  | 'position'
  | 'items'
  | 'summonerSpells'
  | 'primaryRuneTree'
  | 'keystone'
  | 'secondaryRuneTree'
>

export type ChampionSummary = {
  champion: Champion
  buildCount: number
  commentCount: number
  buildsPerPosition: Record<string, number>
  latestBuilds: Build[]
}

export type Comment = {
  id: number
  buildId: number
  text: string
  authorId: number
  authorUsername: string
  createdAt: string
  updatedAt: string
  links: Link[]
}

export type User = { id: number; username: string; email: string; role: 'User' | 'Admin' }

type AuthResponse = { accessToken: string; refreshToken: string; user: User }

// ---------- Errors ----------

// Thrown for every non-2xx response. `fieldErrors` holds validation messages (422).
export class ApiError extends Error {
  status: number
  fieldErrors: Record<string, string[]>

  constructor(status: number, message: string, fieldErrors: Record<string, string[]> = {}) {
    super(message)
    this.status = status
    this.fieldErrors = fieldErrors
  }
}

// ---------- Token storage ----------
// Tokens are kept in localStorage so the user stays logged in after a page reload.

const ACCESS_KEY = 'lb_access'
const REFRESH_KEY = 'lb_refresh'
const USER_KEY = 'lb_user'

export function getStoredUser(): User | null {
  const raw = localStorage.getItem(USER_KEY)
  return raw ? (JSON.parse(raw) as User) : null
}

function saveSession(auth: AuthResponse) {
  localStorage.setItem(ACCESS_KEY, auth.accessToken)
  localStorage.setItem(REFRESH_KEY, auth.refreshToken)
  localStorage.setItem(USER_KEY, JSON.stringify(auth.user))
}

// Called when the session ends (logout or refresh failed). The AuthProvider listens to this.
let onSessionCleared: () => void = () => {}
export function setSessionClearedHandler(handler: () => void) {
  onSessionCleared = handler
}

export function clearSession() {
  localStorage.removeItem(ACCESS_KEY)
  localStorage.removeItem(REFRESH_KEY)
  localStorage.removeItem(USER_KEY)
  onSessionCleared()
}

// ---------- Token refresh ----------
// When a request gets 401 because the access token expired, we swap the refresh token
// for a new pair and retry the request once. If several requests fail at the same time,
// they all wait for the same refresh call.

let refreshPromise: Promise<boolean> | null = null

async function refreshTokens(): Promise<boolean> {
  const refreshToken = localStorage.getItem(REFRESH_KEY)
  if (!refreshToken) return false

  try {
    const res = await fetch(`${API_URL}/api/v1/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    })
    if (!res.ok) return false
    saveSession((await res.json()) as AuthResponse)
    return true
  } catch {
    return false
  }
}

function refreshOnce(): Promise<boolean> {
  if (!refreshPromise) {
    refreshPromise = refreshTokens().finally(() => {
      refreshPromise = null
    })
  }
  return refreshPromise
}

// ---------- Generic request ----------

async function request<T>(method: string, path: string, body?: unknown, canRetry = true): Promise<T> {
  const headers: Record<string, string> = {}
  if (body !== undefined) headers['Content-Type'] = 'application/json'
  const token = localStorage.getItem(ACCESS_KEY)
  if (token) headers['Authorization'] = `Bearer ${token}`

  let res: Response
  try {
    res = await fetch(`${API_URL}${path}`, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
    })
  } catch {
    throw new ApiError(0, 'Could not reach the server. Is the API running?')
  }

  if (res.status === 401 && token && canRetry) {
    const refreshed = await refreshOnce()
    if (refreshed) return request<T>(method, path, body, false)
    clearSession()
    throw new ApiError(401, 'Your session has expired. Please log in again.')
  }

  if (!res.ok) {
    let message = `Request failed (${res.status})`
    let fieldErrors: Record<string, string[]> = {}
    try {
      const problem = await res.json()
      message = problem.detail ?? problem.title ?? message
      fieldErrors = problem.errors ?? {}
    } catch {
      // body was not JSON; keep the default message
    }
    if (res.status === 403 && message === 'Forbidden') message = "You don't have permission to do that."
    throw new ApiError(res.status, message, fieldErrors)
  }

  if (res.status === 204) return undefined as T
  return (await res.json()) as T
}

// ---------- Endpoints ----------

function query(params: Record<string, string | number | undefined>): string {
  const search = new URLSearchParams()
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== '') search.set(key, String(value))
  }
  const text = search.toString()
  return text ? `?${text}` : ''
}

export const api = {
  // auth
  async login(username: string, password: string): Promise<User> {
    const auth = await request<AuthResponse>('POST', '/api/v1/auth/login', { username, password })
    saveSession(auth)
    return auth.user
  },
  async register(username: string, email: string, password: string): Promise<User> {
    const auth = await request<AuthResponse>('POST', '/api/v1/auth/register', { username, email, password })
    saveSession(auth)
    return auth.user
  },
  async logout(): Promise<void> {
    try {
      await request<void>('POST', '/api/v1/auth/logout', undefined, false)
    } finally {
      clearSession()
    }
  },

  // champions
  getChampions: (name: string, page: number, pageSize = 12) =>
    request<Paged<Champion>>('GET', `/api/v1/champions${query({ name, page, pageSize })}`),
  getChampion: (id: number) => request<Champion>('GET', `/api/v1/champions/${id}`),
  getChampionSummary: (id: number) => request<ChampionSummary>('GET', `/api/v1/champions/${id}/summary`),
  createChampion: (input: ChampionInput) => request<Champion>('POST', '/api/v1/champions', input),
  updateChampion: (id: number, input: ChampionInput) => request<Champion>('PUT', `/api/v1/champions/${id}`, input),
  deleteChampion: (id: number) => request<void>('DELETE', `/api/v1/champions/${id}`),

  // builds
  getBuilds: (championId: number, position: string, page: number) =>
    request<Paged<Build>>('GET', `/api/v1/champions/${championId}/builds${query({ position, page, pageSize: 8 })}`),
  getBuild: (championId: number, buildId: number) =>
    request<Build>('GET', `/api/v1/champions/${championId}/builds/${buildId}`),
  createBuild: (championId: number, input: BuildInput) =>
    request<Build>('POST', `/api/v1/champions/${championId}/builds`, input),
  updateBuild: (championId: number, buildId: number, input: BuildInput) =>
    request<Build>('PUT', `/api/v1/champions/${championId}/builds/${buildId}`, input),
  deleteBuild: (championId: number, buildId: number) =>
    request<void>('DELETE', `/api/v1/champions/${championId}/builds/${buildId}`),

  // comments
  getComments: (championId: number, buildId: number, page: number) =>
    request<Paged<Comment>>(
      'GET',
      `/api/v1/champions/${championId}/builds/${buildId}/comments${query({ page, pageSize: 10 })}`,
    ),
  getComment: (championId: number, buildId: number, commentId: number) =>
    request<Comment>('GET', `/api/v1/champions/${championId}/builds/${buildId}/comments/${commentId}`),
  createComment: (championId: number, buildId: number, text: string) =>
    request<Comment>('POST', `/api/v1/champions/${championId}/builds/${buildId}/comments`, { text }),
  updateComment: (championId: number, buildId: number, commentId: number, text: string) =>
    request<Comment>('PUT', `/api/v1/champions/${championId}/builds/${buildId}/comments/${commentId}`, { text }),
  deleteComment: (championId: number, buildId: number, commentId: number) =>
    request<void>('DELETE', `/api/v1/champions/${championId}/builds/${buildId}/comments/${commentId}`),
}
