# Authentication with JWT (Note)

How a user proves who they are on every request, without the server storing a session.

---

## The idea

Login verifies an email + password **once**, then hands the client a **JWT** (JSON
Web Token). The client sends that token on every later request
(`Authorization: Bearer <token>`), and the server trusts it by **checking its
signature** — no session store, no per-request database lookup. This is *stateless
authentication*: the token itself is the proof.

Contrast with server-side sessions, where the server keeps a session table and the
cookie is just a lookup key. Here there is nothing to look up — everything needed
is inside the (signed) token.

---

## Anatomy of a JWT

A token is three Base64URL chunks separated by dots:

```
eyJhbGciOiJIUzI1NiJ9 . eyJzdWIiOiIuLi4iLCJyb2xlIjoiVXNlciJ9 . <signature>
└──── header ───────┘   └──────────── payload ─────────────┘   └─ signature ─┘
```

1. **Header** — the algorithm, e.g. `{ "alg": "HS256", "typ": "JWT" }`.
2. **Payload** — the **claims** (statements about the user). Standard ones:
   `sub` (subject/user id), `iss` (issuer), `aud` (audience), `exp` (expiry),
   `iat` (issued-at). Plus our own: `email`, `role`, `jti`.
3. **Signature** — `HMAC-SHA256(header + "." + payload, secret)`.

**The one insight that makes JWTs click:** the payload is **encoded, not
encrypted**. Anyone holding the token can Base64-decode it and read the claims
(paste it into jwt.io). So **never put secrets in a JWT**. What stops tampering
(e.g. editing `"role":"User"` → `"Admin"`) is the **signature** — you cannot
compute a valid one without the secret, which only the server holds. A JWT gives
**integrity + authenticity**, not confidentiality.

That is also *why it is stateless*: to trust an incoming token the server just
recomputes the signature with its secret and checks `exp` — no DB round-trip.

---

## The two sides: issue and validate

| Side | When | Where | Uses the secret to... |
|------|------|-------|-----------------------|
| **Issue** | at login | `JwtTokenGenerator` (Infrastructure) | **sign** the token |
| **Validate** | every request with a bearer token | JwtBearer middleware (`Program.cs`) | **verify** the signature |

The symmetry is the point: the *same* `Secret` signs and verifies; the *same*
`Issuer`/`Audience` are stamped in at issue-time and checked at validate-time.

### Issue — the port + adapter

The Application layer declares *what* it needs and stays ignorant of *how*:

```csharp
// Application/Abstractions/Security/IJwtTokenGenerator.cs — the port
public interface IJwtTokenGenerator
{
    AccessToken GenerateAccessToken(User user); // AccessToken = (Value, ExpiresAtUtc)
}
```

`Infrastructure/Security/JwtTokenGenerator` is the only file in the codebase that
imports the JWT library. It builds the claims, makes signing credentials from the
secret (HMAC-SHA256), and serializes the `header.payload.signature` string. Same
Ports & Adapters shape as `IPasswordHasher`.

### Validate — the middleware

`Program.cs` wires `AddAuthentication().AddJwtBearer(...)` with the checks every
incoming token must pass before it is trusted:

```csharp
TokenValidationParameters = new()
{
    ValidateIssuerSigningKey = true,  IssuerSigningKey = <key from Jwt:Secret>,
    ValidateIssuer = true,            ValidIssuer   = "gamehub-api",
    ValidateAudience = true,          ValidAudience = "gamehub-client",
    ValidateLifetime = true,          // rejects expired tokens
    RoleClaimType = "role",           // what [Authorize(Roles = "...")] reads
    ClockSkew = TimeSpan.FromSeconds(30)
};
options.MapInboundClaims = false;     // keep claim names as minted (sub/email/role)
```

If validation passes, the middleware builds a `ClaimsPrincipal` and sets
`HttpContext.User`. **Middleware order matters:** `UseAuthentication()` (identify)
must come **before** `UseAuthorization()` (permit).

---

## The claims we mint

| Claim | Value | Purpose |
|-------|-------|---------|
| `sub` | user id (Guid) | the stable, real identity — resolve this → DB when you need truth |
| `email` | user email | convenience (display/logging) without a DB hit |
| `role` | `User` / `Admin` | coarse RBAC; read by `[Authorize(Roles = "...")]` |
| `jti` | random Guid | unique token id; useful later for revocation / refresh |
| `iss` `aud` `exp` `iat` | from settings / clock | standard issuer / audience / expiry / issued-at |

---

## Decisions & their trade-offs

### Vague error on failed login
Login returns the **same** `401 "The email or password is incorrect."` whether the
email is unknown or the password is wrong. Revealing "no such email" would let an
attacker **enumerate** which accounts exist. (Contrast RegisterUser, where
"email already taken" is fine to reveal — different threat.)

### Role lives in the token (snapshot), not read from the DB per request
`[Authorize(Roles = "Admin")]` reads the role **from the token**, so authorization
needs **zero DB lookups** — the main performance win of JWT. The cost is
**staleness**: the token is a snapshot from login, so demoting an admin does not
take effect until their token expires. Mitigations, in order of what we use:

1. **Short access-token lifetime** (`AccessTokenMinutes = 60`) shrinks the window.
2. **DB re-check for genuinely sensitive actions only** — pay the query where it matters.
3. (Alternative design) token carries only `sub`, authorize by loading the user
   every request — always current, but reintroduces per-request state. Reserved
   for cases needing instant revocation.

A **refresh-token** flow (future slice) re-mints the token with fresh claims,
which is what makes a short access-token lifetime practical.

---

## Refinements

- **Timing side-channel — DONE.** The unknown-email path used to return immediately
  while the found-user path ran bcrypt (slow by design), so response timing could leak
  whether an email existed. `LoginHandler` now runs a throwaway `Verify` against a fixed
  dummy hash (`DummyPasswordHash`, work factor 12) on the null-user path, so both
  credential-failure paths cost roughly the same. Measured: unknown-email ≈ wrong-password
  ≈ 0.26s; a validation failure (no bcrypt) ≈ 0.006s.
- **Email case-sensitivity** (still deferred): `GetByEmailAsync` matches exactly, and registration
  stores the email as typed (trimmed). `Ben@x.com` cannot log in as `ben@x.com`.
  The whole app is consistently case-sensitive on email today; normalizing is its
  own future change across register + login.

---

## Where it lives (the Login slice)

```
API            Contracts/Auth/LoginRequest, Controllers/AuthController   → POST /api/auth/login
API            Program.cs                                                → AddJwtBearer + UseAuthentication
Application    Authentication/Login/{LoginCommand,Validator,Handler,Response}
Application    Authentication/AuthErrors                                 → InvalidCredentials (401)
Application    Abstractions/Security/{IJwtTokenGenerator, AccessToken}   → the port
Infrastructure Security/{JwtTokenGenerator, JwtSettings}                 → the adapter + typed config
```

See `11-secrets-and-configuration.md` for the signing secret, `05-vertical-slice-recipe.md`
for the slice shape, and `06-request-lifecycle-and-di.md` for service lifetimes.
