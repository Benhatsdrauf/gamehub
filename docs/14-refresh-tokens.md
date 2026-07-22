# Refresh Tokens (Note)

How a session outlives a 15-minute access token, and how the system gains real
logout and theft protection.

---

## The idea

The access token is deliberately short-lived (15 min) and **stateless** — nothing
about it is stored, so it **cannot be revoked**; it just expires. That is great for
performance (authorize with zero DB lookups) but bad for two things: users would be
kicked out every 15 minutes, and a stolen token works until it expires no matter what.

A **refresh token** fixes both. It is a second token, long-lived and **stored**
(so revocable), used only to mint new access tokens.

| | Access token | Refresh token |
|---|---|---|
| Lifetime | 15 min | 7 days |
| Sent | every request | only to `/auth/refresh` |
| Stored server-side | no (stateless) | **yes** — only its hash |
| Revocable | no (expires) | **yes** (delete/revoke the row) |
| Job | authorize requests | mint new access tokens without re-login |

So the access token stays fast and unrevocable-but-brief; the refresh token, used
rarely and revocable, is what gives us logout and theft response.

---

## The stored entity: `RefreshToken`

A domain entity (Domain/Authentication) whose fields *are* the feature:

| Field | Meaning |
|---|---|
| `TokenHash` | SHA-256 of the token — the raw token is **never stored** |
| `CreatedAtUtc` / `ExpiresAtUtc` | issued / expiry timestamps |
| `RevokedAtUtc` | set when killed (rotation, logout, theft); null = alive |
| `ReplacedByTokenId` | when rotated, points at the successor token |

Behavior (the entity owns its own transitions — nothing sets these from outside):

- `IsActive(utcNow)` → not revoked **and** not expired
- `WasRotated` → `ReplacedByTokenId` is set — this token was already used and rotated away
- `Revoke(utcNow, replacedByTokenId?)` → kill it, optionally linking its successor

Time is passed **in** (not read from the clock) so the entity stays pure and testable.

---

## The three flows

### Login — issue
After verifying the password, `LoginHandler` also mints a refresh token and stores
its hash, returning both tokens (with expiries) to the client:

```
access  = jwt.GenerateAccessToken(user)
refresh = refreshGen.Create(user.Id)   // raw string + entity (hash + expiry baked in)
refreshTokens.Add(refresh.Token); SaveChanges()
→ { accessToken, refreshToken.RawToken, both expiries, user info }
```

### Refresh — rotate (`POST /auth/refresh`)
```
hash the incoming token → look it up
├─ not found              → 401
├─ found but NOT active:
│     ├─ WasRotated  → REPLAY of a used token = theft
│     │      → revoke ALL the user's live tokens, then 401
│     └─ else (just expired) → 401
└─ found and active → ROTATE:
      load user → mint new access + new refresh
      Add(newRefresh);  old.Revoke(now, newRefresh.Id)   ← link the chain
      SaveChanges  (revoke-old + add-new in one transaction)
      → { new access, new refresh }
```

### Logout (`POST /auth/logout`)
Hash → look up → if alive, `Revoke(now)` + save. Returns **204 either way** —
idempotent, and never reveals whether the token existed. The access token then dies
on its own within 15 minutes.

Both endpoints are `[AllowAnonymous]`: the refresh token itself is the credential
(the access token may already be expired when you refresh or log out).

---

## Rotation & reuse detection

**Rotate on every use:** each `/refresh` issues a new token and revokes the old one,
so any single token is valid for exactly one refresh. This shrinks the window a
stolen token is useful.

**Reuse detection:** once a token is rotated, the legitimate client always moves on
to its successor. So if an *already-rotated* token (`WasRotated`) is presented again,
someone is replaying a used token — a theft signal. Response: **revoke every live
token for that user** (log the whole session out). You cannot mint from a stolen
refresh token for long, and the moment it is replayed the session dies.

---

## Why SHA-256 (not bcrypt) for the hash

Passwords are hashed with bcrypt because they are **low-entropy** — bcrypt's
slowness is what makes guessing them infeasible. A refresh token is a **256-bit
random** value; there is nothing to guess or brute-force regardless of hash speed.
So a fast, deterministic **SHA-256** is the correct choice — and deterministic
matters because we look a token up *by* its hash. (bcrypt is also salted/slow, which
would make lookup impossible and pointless here.)

Only the hash is stored, so a database leak exposes hashes, not usable credentials.

---

## Config

`appsettings.json` → `Jwt` section: `AccessTokenMinutes = 15`, `RefreshTokenDays = 7`.
The refresh lifetime is hidden from the Application by `IRefreshTokenGenerator.Create`
(which bakes expiry into the entity), exactly as `IJwtTokenGenerator` hides the access
lifetime.

---

## Where it lives

```
Domain          Authentication/RefreshToken
Application     Abstractions/Security/{IRefreshTokenGenerator, NewRefreshToken}
Application     Authentication/{IRefreshTokenRepository, AuthErrors.InvalidRefreshToken}
Application     Authentication/Refresh/{Command, Response, Validator, Handler}
Application     Authentication/Logout/{Command, Validator, Handler}
Infrastructure  Security/RefreshTokenGenerator; Persistence/{RefreshTokenConfiguration,
                Repositories/RefreshTokenRepository}; Migrations/AddRefreshTokens
API             AuthController: POST /auth/refresh, POST /auth/logout
```

---

## Known refinements (deliberately deferred)

- **Per-device token families.** Reuse detection currently revokes *all* of a user's
  tokens — a theft on one device logs out every device. A `FamilyId` (stamped at login,
  carried through rotations) would scope revocation to just the compromised chain.
- **Cookie transport.** The refresh token travels in the request/response body for now
  (backend-first, Postman-testable). The production-hardened transport is an httpOnly,
  Secure, SameSite cookie so JavaScript can't read it (XSS resistance) — a frontend
  decision for when the SPA exists.
- **Clock abstraction.** Handlers/generator read `DateTime.UtcNow` directly; a
  `TimeProvider` would make time-dependent paths fully deterministic in tests.

See `10-authentication-jwt.md` for the access-token/JWT side and `12-mediator.md` for
how the Refresh/Logout slices dispatch.
