# Vertical Slice Recipe

A reusable, step-by-step procedure for implementing one feature ("slice")
end-to-end. Use it as a checklist next to any new slice.

The worked example throughout is **Register User** (the first slice).

---

## Mental Model

Every feature is one thin vertical cut through all four layers, rather than a
horizontal change spread across a "controllers folder", a "services folder", etc.

```
Request  →  Command/Query  →  Handler  →  (Domain + Abstractions)  →  Response  →  HTTP
```

Everything else is detail hanging off that spine.

---

## Contracts: the shapes at each boundary

A **contract** is an agreed-upon shape of data at a boundary. Data crosses three
boundaries in a slice, so there are three contracts — each owned by a different
layer. This is why we have both a `Request` and a `Command` even when they look
identical: they are promises made by different layers.

| Boundary                    | Contract (shape)                              | Owned by       |
|-----------------------------|-----------------------------------------------|----------------|
| Client ↔ **API**            | `XxxRequest` (in) + JSON response (out)        | API layer      |
| API ↔ **Application**       | `XxxCommand`/`XxxQuery` (in) + `XxxResponse`   | Application    |
| Application ↔ **Infra**     | `IXxxRepository`, service interfaces           | Application (Infra implements) |

The **controller translates** the API contract into the Application contract.
Keeping them separate means the HTTP shape and the application shape can evolve
independently without breaking each other.

### Data flow (Register User)

```
CLIENT
  │  JSON { username, email, password }
  ▼ ───────────────────────── API contract (RegisterUserRequest)
CONTROLLER  maps Request → Command
  │  RegisterUserCommand { Username, Email, Password }   (still plaintext)
  ▼ ───────────────────────── Application contract (in)
HANDLER
  │  hashes password → new User(username, email, hash)
  ▼ ───────────────────────── Domain (invariants enforced)
  │  IUserRepository.AddAsync(user)
  ▼ ───────────────────────── Infra contract (interface → EF impl)
POSTGRES   (row saved; PasswordHash = $2...)
  ▲
  │  handler maps User → RegisterUserResponse { Id, Username, Email }   (NO hash)
  ▲ ───────────────────────── Application contract (out)
CONTROLLER  returns 201 + response
  ▲
  │  JSON { id, username, email }
CLIENT
```

The password shape degrades safely inbound (plaintext → hash → gone) and the
hash never appears outbound. Each contract is a checkpoint controlling what may
cross.

---

## The Recipe

Walk these steps in order for any new slice.

### 1. Name the use case and its contracts (the "what")
- **API contract:** what does the client send? → `XxxRequest` in `API/Contracts/`.
- **Application contract:** `XxxCommand` (writes) or `XxxQuery` (reads),
  plus a `XxxResponse` DTO. **Never return the domain entity** — DTOs prevent
  leaking internal fields (e.g. `PasswordHash`) and decouple the wire from the domain.

### 2. Identify the handler's dependencies (the "capabilities")
- Needs to load/save data? → a repository method (reuse or add to `IXxxRepository`).
- Needs a service (hashing, email, clock)? → an interface in `Application/Abstractions/`.
- **Define interfaces you don't have; reuse the ones you do.** The Application
  layer *owns* these interfaces; Infrastructure implements them (dependency inversion).

### 3. Write the handler (the "how" — the brain)
- Inject the abstractions (interfaces only — never a concrete DbContext or library).
- Enforce business rules → return `Result.Failure(SomeErrors.X())` for **expected** failures.
- Call domain behavior (`new Entity(...)` or `entity.DoThing()`).
- Call the abstraction to persist.
- Map entity → `XxxResponse`; return `Result.Success(...)`.

### 4. Add domain behavior if the rule belongs there
- New invariant or state transition → a method on the entity, keeping the domain
  the source of truth. The domain knows nothing about EF, hashing, or HTTP.

### 5. Implement the abstractions in Infrastructure
- EF Core repository method / external service. The only place infra libraries appear.

### 6. Register in DI with correct lifetimes
- Handler → `AddApplication()` (Scoped).
- Implementation → `AddInfrastructure()`:
  - **Scoped** if it holds a `DbContext` (which is scoped and not thread-safe).
  - **Singleton** if stateless and thread-safe.
- Rule: a service must **never outlive its dependencies** (avoid captive dependencies).

### 7. Add the controller action (thin)
- Map `Request → Command`, call `Handle`, then:
  `return result.IsSuccess ? <success shape> : Problem(result.Error);`
- Success shape is per-endpoint (`201 Created`, `200 OK`, ...).
- Failure mapping is shared once in `ApiController.Problem` (ErrorType → status + ProblemDetails).

### 8. Wire references/packages, then build → run → test
- A project should **reference what it directly uses** (not rely on transitive references).
- Test the happy path **and** each failure path (e.g. the 409 conflict).

---

## Error Handling Convention

- **Expected business outcome** (email taken, review on unpublished game) → return a `Result` failure.
- **Domain invariant violation** (empty username reaching the entity) → the entity throws (a bug slipped past validation).
- **Infrastructure failure** (DB down) → let it throw; middleware handles it.

`Error` carries a semantic `ErrorType` (Validation, Conflict, NotFound, ...),
**not** an HTTP status code. The API layer alone maps `ErrorType → status`.

---

## Uniqueness / Concurrency Rule

- **Application-level check** (e.g. `EmailExistsAsync`) → friendly error in the common case (UX).
- **Database unique index** → the correctness guarantee that survives a race
  (TOCTOU: two requests can both pass the check, so only the DB can truly enforce it).

Never trust the app check alone; never rely only on the DB error for everyday UX.

---

## One-Line Mnemonic

**Request → Command → Handler → (Domain + Abstractions) → Response → HTTP.**
