// Where the two tokens live on the client:
//  - access token: in MEMORY only (module variable). Short-lived, and keeping it out
//    of localStorage limits XSS exposure. Lost on reload — that's fine, we silently
//    re-mint it from the refresh token on startup.
//  - refresh token: localStorage, so a page reload can restore the session.
//
// (The stronger production option is an httpOnly cookie the JS can't read; that needs
//  backend cookie support and is a later hardening — see docs/14-refresh-tokens.md.)

const REFRESH_KEY = 'gh_refresh_token'

let accessToken: string | null = null

export const tokenStore = {
  getAccess: (): string | null => accessToken,

  getRefresh: (): string | null => localStorage.getItem(REFRESH_KEY),

  setTokens: (access: string, refresh: string): void => {
    accessToken = access
    localStorage.setItem(REFRESH_KEY, refresh)
  },

  clear: (): void => {
    accessToken = null
    localStorage.removeItem(REFRESH_KEY)
  },
}
