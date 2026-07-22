import type { ZodType } from 'zod'
import { API_URL } from '@/lib/config'
import { refreshRequest } from '@/lib/api/auth'
import { toApiError } from '@/lib/api/error'
import { tokenStore } from '@/lib/auth/tokenStore'

// The AuthProvider registers a callback here so a failed background refresh can clear
// the session (and bounce the user to login) from anywhere.
let authFailureHandler: (() => void) | null = null
export function setAuthFailureHandler(handler: (() => void) | null): void {
  authFailureHandler = handler
}

// Single-flight refresh: if ten requests 401 at once, only ONE /refresh runs; the rest
// await the same promise. Critical with rotation — two concurrent refreshes would each
// consume a token and the second would look like reuse (theft) and nuke the session.
let refreshInFlight: Promise<boolean> | null = null

function ensureRefreshed(): Promise<boolean> {
  const refreshToken = tokenStore.getRefresh()
  if (!refreshToken) return Promise.resolve(false)

  refreshInFlight ??= (async () => {
    try {
      const tokens = await refreshRequest(refreshToken)
      tokenStore.setTokens(tokens.accessToken, tokens.refreshToken)
      return true
    } catch {
      tokenStore.clear()
      return false
    } finally {
      refreshInFlight = null
    }
  })()

  return refreshInFlight
}

// Authenticated fetch: attaches the access token, and on a 401 transparently refreshes
// once and retries the original request.
async function apiFetch(path: string, options: RequestInit, allowRetry = true): Promise<Response> {
  const headers = new Headers(options.headers)

  const access = tokenStore.getAccess()
  if (access) headers.set('Authorization', `Bearer ${access}`)
  if (options.body && !headers.has('Content-Type')) headers.set('Content-Type', 'application/json')

  const response = await fetch(`${API_URL}${path}`, { ...options, headers })

  if (response.status === 401 && allowRetry && tokenStore.getRefresh()) {
    const refreshed = await ensureRefreshed()
    if (refreshed) return apiFetch(path, options, false)
    authFailureHandler?.()
  }

  return response
}

// Typed GET/POST that returns parsed, validated JSON (throws ApiError on failure).
export async function apiRequest<T>(
  path: string,
  schema: ZodType<T>,
  options: RequestInit = {},
): Promise<T> {
  const response = await apiFetch(path, options)
  if (!response.ok) throw await toApiError(response)
  return schema.parse(await response.json())
}

// For endpoints with no response body (e.g. 204 No Content).
export async function apiSend(path: string, options: RequestInit = {}): Promise<void> {
  const response = await apiFetch(path, options)
  if (!response.ok) throw await toApiError(response)
}
