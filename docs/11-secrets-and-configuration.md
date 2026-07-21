# Secrets & Configuration (Note)

Where settings come from, and how secrets stay out of git.

---

## The idea

`IConfiguration` (what `builder.Configuration` is) is **not one file** — it is a
**layered stack**. Each layer can override keys from the layer before, and code
reads a key (`configuration["Jwt:Secret"]`) without caring which layer supplied it.

Later layers win:

```
appsettings.json                 ← committed; non-secret defaults
   ▼ overridden by
appsettings.{Environment}.json   ← committed; per-environment non-secret overrides
   ▼ overridden by
User Secrets                     ← DEV only; stored OUTSIDE the repo (never committed)
   ▼ overridden by
Environment variables            ← PROD; secrets injected by the host
```

The rule that falls out of this: **non-secret settings live in `appsettings`
(committed); secrets live in user-secrets (dev) or environment variables (prod) —
never in a committed file.**

---

## What goes where

| Setting | Sensitive? | Lives in |
|---------|------------|----------|
| `Jwt:Issuer`, `Jwt:Audience`, `Jwt:AccessTokenMinutes` | No | `appsettings.json` |
| `Jwt:Secret` (signing key) | **Yes** | user-secrets (dev) / env var (prod) |
| `ConnectionStrings:DefaultConnection` | Yes-ish (dev creds only) | `appsettings.json` for now |

`.NET`'s dev-secret tool is **`dotnet user-secrets`**. It is the built-in
equivalent of a Node/Nest `.env` file: its job is to keep dev secrets out of git.
The difference is *where* it stores them — see below.

---

## How to add a secret (general recipe)

Run from the project that reads the secret (here, the API):

```bash
# 1. One-time: give the project a secrets store (adds <UserSecretsId> to the .csproj)
dotnet user-secrets init --project src/GameHub.API/GameHub.API.csproj

# 2. Set a secret. The key uses ":" to descend into config sections.
dotnet user-secrets set "Jwt:Secret" "<a long random value>" --project src/GameHub.API/GameHub.API.csproj

# Inspect / remove
dotnet user-secrets list   --project src/GameHub.API/GameHub.API.csproj
dotnet user-secrets remove "Jwt:Secret" --project src/GameHub.API/GameHub.API.csproj
```

**Where it is stored:** *not* in the repo. On macOS/Linux:
`~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`. The `<UserSecretsId>` in
the `.csproj` is only a **pointer** to that folder — it is safe to commit; the
secret value never is. User-secrets are read **only in the Development
environment**.

---

## How we did it here (the JWT signing secret)

```bash
dotnet user-secrets init --project src/GameHub.API/GameHub.API.csproj
# → wrote <UserSecretsId>c9460d53-...-fe5a28</UserSecretsId> into the .csproj

SECRET=$(openssl rand -base64 48)        # a strong random key (HMAC-SHA256 wants >= 256 bits)
dotnet user-secrets set "Jwt:Secret" "$SECRET" --project src/GameHub.API/GameHub.API.csproj
```

That `Jwt:Secret` overlays the `Jwt` section already in `appsettings.json`
(which holds only issuer/audience/expiry), so the bound `JwtSettings` object ends
up with **all** its fields — the non-secret ones from the committed file, the
secret one from the store. `JwtTokenGenerator` signs with it; the JwtBearer
middleware verifies with the same value (see `10-authentication-jwt.md`).

> If a teammate clones the repo, the app will fail fast at startup with
> "Jwt settings are missing from configuration" until they run the two commands
> above with their own secret. That is intentional — there is no secret to leak in
> the repo.

---

## Production: environment variables

In production there is no user-secrets. `.NET` reads environment variables as the
top config layer automatically, and maps a **double underscore** `__` to the
config `:` separator:

```bash
# These override the same keys the app already reads — no code change.
export Jwt__Secret="<production signing key from the secret manager>"
export ConnectionStrings__DefaultConnection="Host=...;Password=..."
```

So the same `configuration["Jwt:Secret"]` transparently resolves from
user-secrets in dev and from an env var in prod. Real deployments source these
from a secret manager (Docker/Kubernetes secrets, AWS/Azure vaults) rather than a
plain shell export.

---

See `10-authentication-jwt.md` for what the secret signs, and `06-request-lifecycle-and-di.md`
for how `JwtSettings` is bound and injected (`IOptions<JwtSettings>`).
