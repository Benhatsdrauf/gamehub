// We derive the current user by decoding the access-token JWT — the sub/email/role
// claims are already in it, so there's no need for a /me endpoint. This is for UI
// only (deciding what to show); the server still enforces authorization for real.

export interface AccessTokenClaims {
  sub: string
  email: string
  role: string
  exp: number
}

function base64UrlDecode(segment: string): string {
  const base64 = segment.replace(/-/g, '+').replace(/_/g, '/')
  const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=')
  return atob(padded)
}

export function decodeAccessToken(token: string): AccessTokenClaims | null {
  try {
    const payload = token.split('.')[1]
    if (!payload) return null

    const claims = JSON.parse(base64UrlDecode(payload)) as Record<string, unknown>
    if (typeof claims.sub !== 'string') return null

    return {
      sub: claims.sub,
      email: typeof claims.email === 'string' ? claims.email : '',
      role: typeof claims.role === 'string' ? claims.role : 'User',
      exp: typeof claims.exp === 'number' ? claims.exp : 0,
    }
  } catch {
    return null
  }
}
