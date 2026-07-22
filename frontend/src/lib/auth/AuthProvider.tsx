import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from 'react'
import { loginRequest, logoutRequest, refreshRequest } from '@/lib/api/auth'
import { setAuthFailureHandler } from '@/lib/api/client'
import { decodeAccessToken } from '@/lib/auth/jwt'
import { tokenStore } from '@/lib/auth/tokenStore'

export interface AuthUser {
  id: string
  email: string
  role: string
}

export interface AuthContextValue {
  user: AuthUser | null
  isAuthenticated: boolean
  isAdmin: boolean
  // True while we attempt a silent refresh on startup — the app should wait for this
  // before deciding "logged out" and redirecting.
  isBootstrapping: boolean
  login: (email: string, password: string) => Promise<void>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

function userFromAccessToken(): AuthUser | null {
  const access = tokenStore.getAccess()
  if (!access) return null

  const claims = decodeAccessToken(access)
  if (!claims) return null

  return { id: claims.sub, email: claims.email, role: claims.role }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null)
  const [isBootstrapping, setIsBootstrapping] = useState(true)

  const clearSession = useCallback(() => {
    tokenStore.clear()
    setUser(null)
  }, [])

  useEffect(() => {
    // Let the API client clear the session if a background refresh ever fails.
    setAuthFailureHandler(clearSession)

    // Startup: the access token was lost on reload, but if a refresh token survived in
    // localStorage, silently mint a new access token so the session continues.
    const refreshToken = tokenStore.getRefresh()
    if (refreshToken) {
      refreshRequest(refreshToken)
        .then((tokens) => {
          tokenStore.setTokens(tokens.accessToken, tokens.refreshToken)
          setUser(userFromAccessToken())
        })
        .catch(() => clearSession())
        .finally(() => setIsBootstrapping(false))
    } else {
      setIsBootstrapping(false)
    }

    return () => setAuthFailureHandler(null)
  }, [clearSession])

  const login = useCallback(async (email: string, password: string) => {
    const response = await loginRequest(email, password)
    tokenStore.setTokens(response.accessToken, response.refreshToken)
    setUser(userFromAccessToken())
  }, [])

  const logout = useCallback(async () => {
    const refreshToken = tokenStore.getRefresh()
    if (refreshToken) {
      try {
        await logoutRequest(refreshToken)
      } catch {
        // best-effort; clear locally regardless
      }
    }
    clearSession()
  }, [clearSession])

  const value: AuthContextValue = {
    user,
    isAuthenticated: user !== null,
    isAdmin: user?.role === 'Admin',
    isBootstrapping,
    login,
    logout,
  }

  return <AuthContext value={value}>{children}</AuthContext>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth must be used within an AuthProvider')
  return context
}
