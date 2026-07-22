import { API_URL } from '@/lib/config'
import { toApiError } from '@/lib/api/error'
import {
  loginResponseSchema,
  refreshResponseSchema,
  type LoginResponse,
  type RefreshResponse,
} from '@/lib/api/schemas'

// The auth endpoints are [AllowAnonymous] and must NOT go through the client's
// refresh-on-401 interceptor (a 401 from /login means "bad credentials", not "token
// expired"). So they use plain fetch.
async function postJson(path: string, body: unknown): Promise<Response> {
  return fetch(`${API_URL}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
}

export async function loginRequest(email: string, password: string): Promise<LoginResponse> {
  const response = await postJson('/auth/login', { email, password })
  if (!response.ok) throw await toApiError(response)
  return loginResponseSchema.parse(await response.json())
}

export async function refreshRequest(refreshToken: string): Promise<RefreshResponse> {
  const response = await postJson('/auth/refresh', { refreshToken })
  if (!response.ok) throw await toApiError(response)
  return refreshResponseSchema.parse(await response.json())
}

export async function logoutRequest(refreshToken: string): Promise<void> {
  // Best-effort: the server is idempotent and returns 204 regardless.
  await postJson('/auth/logout', { refreshToken })
}
