# Docker & Compose (Note)

How to run the whole stack — Postgres + .NET API + React SPA — with one command, in
either a production-like or a hot-reload dev mode.

---

## Two workflows, two commands

| | Command | Frontend | Backend | Hot reload? |
|---|---|---|---|---|
| **Production** | `docker compose up --build` | nginx serving the built bundle | published, slim runtime | no |
| **Dev** | `docker compose -f compose.dev.yaml up` | Vite dev server (HMR) | `dotnet watch` | **yes** |

Both bring up all three services. Ports are the same either way:
**web** `:5173` (dev) / `:3000` (prod) · **api** `:5206` · **db** `:5432`.

> Only run one at a time — they both bind `:5432`/`:5206` (and prod also `:3000`, dev `:5173`).

---

## Production (`compose.yaml`) — built images

Each app is compiled into an image; `docker compose up --build` runs them as they'd run
in production.

- **`backend/Dockerfile`** is **multi-stage**: a big SDK image compiles and `publish`es
  the app, then only the output is copied into a slim `aspnet` runtime image (no SDK,
  much smaller). The `.csproj` files are copied and restored *before* the source, so the
  restore layer is cached until dependencies change.
- **`frontend/Dockerfile`** builds the static bundle with Node, then copies `dist/` into
  an `nginx` image. `nginx.conf` has an **SPA fallback** (`try_files … /index.html`) so
  client-side routes like `/users` work on direct navigation and refresh.

### The browser-vs-container URL subtlety
`VITE_API_URL` is a **build ARG**, baked into the bundle. It must be the **host-reachable**
API URL (`http://localhost:5206/api`) — *not* the internal `api` service hostname —
because the code runs in **your browser**, not the web container. The browser reaches the
API through the published host port.

---

## Dev (`compose.dev.yaml`) — hot reload

Uses **stock** SDK/Node images (no image build) with your **source bind-mounted**, so
edits on the host trigger a rebuild/refresh inside the container.

- **api**: `dotnet watch … run` in the SDK image. `DOTNET_USE_POLLING_FILE_WATCHER=true`
  because inotify events don't cross Docker bind mounts reliably on macOS/Windows.
- **web**: `pnpm dev` (Vite HMR). `server.host: true` (in `vite.config.ts`) makes it
  reachable from outside the container; `VITE_USE_POLLING=true` enables polling for the
  same bind-mount reason.
- **Container-only artifacts**: anonymous volumes shadow each project's `bin`/`obj` and the
  SPA's `node_modules`, so linux (container) and macOS (host) build outputs don't overwrite
  each other. First `up` is slower (restore + `pnpm install`); after that, saves hot-reload.

**Lighter alternative:** run *just* the database in Docker and the apps natively —
`docker compose up db` (or `-f compose.dev.yaml up db`), then `dotnet watch` in `backend/`
and `pnpm dev` in `frontend/`. This is the fastest, least-fiddly hot reload on macOS and
what many devs use daily.

---

## Configuration (env, not files)

The containerized API is configured entirely by environment variables (compose sets them):

- `ConnectionStrings__DefaultConnection` → **`Host=db`** (the compose service name), not
  `localhost` — containers talk over the compose network.
- `Jwt__Secret` → a **dev-only** value (user-secrets isn't available in a container). Not
  for production.
- `Cors__AllowedOrigins__0/1` → the web origins (`:3000` prod, `:5173` dev).
- `RunMigrationsOnStartup=true` → the API applies EF migrations at boot (see below).
- `SeedAdmin=true` → seed a default admin (see below).

The `__` (double underscore) maps to the config `:` separator — `Jwt__Secret` sets
`Jwt:Secret`. See `11-secrets-and-configuration.md`.

---

## Migrate + seed on startup

A fresh container database is empty, so `Program.cs` (gated by config, dev-only):

1. **Migrates** — `RunMigrationsOnStartup=true` runs `dbContext.Database.Migrate()` after
   the DB healthcheck passes, creating the schema. No manual `dotnet ef database update`.
2. **Seeds an admin** — `SeedAdmin=true` runs `DatabaseSeeder`, which creates a default
   admin if one doesn't exist (idempotent). Credentials come from `Seed:Admin*` config,
   defaulting to **`admin@gamehub.local` / `Admin123!`**.

So after `docker compose up --build`, open http://localhost:3000 and log in with the
seeded admin immediately — no bootstrap curl needed.

---

## Gotchas

- **Run one stack at a time.** Prod and dev both bind `:5432`/`:5206`. Also stop any
  native backend / old `gamehub-postgres` container first (`docker rm -f gamehub-postgres`).
- **Fresh volume = fresh data.** The compose DB uses its own volume; users created in one
  stack aren't in the other. The seed admin is recreated automatically.
- **CORS error in the browser ≠ server error.** If the SPA can't reach the API, check the
  origin is allow-listed (`Cors__AllowedOrigins`). See `15-cors.md`.
- **Dev hot reload on macOS needs polling** (already set) — without it, file saves inside
  bind mounts may not be detected.

See `11-secrets-and-configuration.md` for the env/config model and `15-cors.md` for CORS.
