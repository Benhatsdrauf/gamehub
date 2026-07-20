# Request Lifecycle & Dependency Injection

How an HTTP request travels from the wire to a handler and back — and how
Dependency Injection (DI) constructs the objects along the way.

The worked example is `POST /api/users` (Register User).

---

## The two engines

Two engines do work you never call directly. Understand these and the rest
follows:

1. **The DI container** — builds objects *and* their dependencies for you.
2. **The MVC pipeline** — routes the request, turns JSON into objects (model
   binding), and turns your return value back into JSON (serialization).

You only ever write the **mapping** and the **orchestration**. The engines
handle construction and transport.

---

## Part 1 — Dependency Injection in general

### The problem it solves: Inversion of Control

Without DI, a class builds its own collaborators:

```csharp
// tightly coupled — the controller knows how to build everything
var handler = new RegisterUserHandler(
    new RegisterUserCommandValidator(),
    new UserRepository(new GameHubDbContext(...)),
    new PasswordHasher());
```

The controller now depends on every *concrete* type and every constructor
signature down the tree. Change any of them and the controller breaks.

With DI we **invert** this: a class *declares what it needs* (via its
constructor) and the container *supplies it*.

```csharp
public UsersController(RegisterUserHandler handler)  // "I need a handler"; not "here's how to build one"
```

This is **Inversion of Control**: you ask for *what*, not *how*.

### Registration = teaching the container recipes

At startup we record recipes (no objects are created yet):

```csharp
builder.Services.AddApplication();          // RegisterUserHandler, validators
builder.Services.AddInfrastructure(config); // IUserRepository→UserRepository, IPasswordHasher→PasswordHasher, DbContext
```

A recipe maps a **requested type** to **how to produce it** — often an
interface to a concrete implementation:

```csharp
services.AddScoped<IUserRepository, UserRepository>();     // "need IUserRepository? build UserRepository"
services.AddSingleton<IPasswordHasher, PasswordHasher>();
services.AddScoped<RegisterUserHandler>();                 // concrete type, no interface needed
```

### Resolution = building the tree on demand

When something needs a type, the container reads that type's constructor and
recursively builds its dependencies:

```
UsersController
  └─ RegisterUserHandler
       ├─ IValidator<RegisterUserCommand>  → RegisterUserCommandValidator
       ├─ IUserRepository                  → UserRepository
       │                                        └─ GameHubDbContext
       └─ IPasswordHasher                  → PasswordHasher
```

You never write `new RegisterUserHandler(...)`. The container does, wired with
every dependency.

### Service lifetimes

A recipe also says *how long* an instance lives:

| Lifetime      | One instance per…      | Use for                                            |
|---------------|------------------------|----------------------------------------------------|
| **Transient** | every request for it   | cheap, stateless helpers                           |
| **Scoped**    | one HTTP request       | anything holding a `DbContext` (per-request state) |
| **Singleton** | the whole application  | stateless, thread-safe services                    |

In GameHub:
- `PasswordHasher` → **Singleton** (stateless, thread-safe).
- `UserRepository` → **Scoped** (holds a `DbContext`, which is scoped and not thread-safe).
- `RegisterUserHandler` → **Scoped** (depends on the scoped repository).

### The captive dependency rule

> A service must never outlive its dependencies.

If `UserRepository` were a Singleton, it would capture **one** `DbContext`
forever and share it across all requests and threads — corrupting EF change
tracking. That is a *captive dependency*. The repository is Scoped precisely
to match the lifetime of the `DbContext` it holds.

### Why we inject interfaces

Depending on `IUserRepository` instead of `UserRepository` means the handler
does not know about EF Core. Infrastructure implements the interface; the
dependency arrow points **inward** (see architecture doc). It also makes the
handler unit-testable with a fake repository.

---

## Part 2 — The request lifecycle

### Step 0 — Startup records the recipes
`Program.cs` runs `AddControllers()`, `AddApplication()`, `AddInfrastructure()`.
Nothing is built yet; the container just knows the dependency graph.

### Step 1 — Routing selects the action
`POST /api/users` matches:
- `[Route("api/[controller]")]` on `ApiController` → `UsersController` = `api/users`
- `[HttpPost]` on `Register` → the POST verb

Decision: run `UsersController.Register`. But an *instance* is needed first.

### Step 2 — DI builds the controller and its whole tree
The container reads `UsersController`'s constructor (`RegisterUserHandler`),
then the handler's constructor (validator, repository, hasher), then the
repository's (`GameHubDbContext`) — building the full tree from Part 1.
Scoped services share one instance for this request.

### Step 3 — Model binding turns JSON into a request object
The action parameter `RegisterUserRequest request` is a complex type, so
(`[ApiController]`) the pipeline deserializes the JSON body into a new
`RegisterUserRequest`. The `CancellationToken` is supplied by the framework
(the "client disconnected" signal). Both exist before your code runs.

### Step 4 — Your controller code runs (the only part you wrote)

```csharp
var command = new RegisterUserCommand(      // (a) map API contract → application contract
    request.Username, request.Email, request.Password);

var result = await _registerUserHandler.Handle(command, cancellationToken); // (b) orchestrate

return result.IsSuccess                     // (c) map Result → HTTP
    ? Created($"/api/users/{result.Value.Id}", result.Value)
    : Problem(result.Error);
```

- **(a)** the only `new` you write — crossing the wire→application boundary.
- **(b)** `_registerUserHandler` is the instance DI injected in Step 2.
- **(c)** success → 201 Created; failure → `Problem` (ValidationError → 400,
  otherwise `ErrorType` → status via `ProblemDetails`).

### Step 5 — MVC serializes the result back to HTTP
The returned `IActionResult` is serialized to JSON, the status code is set, and
the response is written to the client.

### Diagram

```
CLIENT ── POST /api/users (JSON) ──▶ MVC: routing
                                       │
                                       ▼
                                 DI: build UsersController + handler tree
                                       │
                                       ▼
                                 MVC: bind JSON → RegisterUserRequest
                                       │
                                       ▼
                    CONTROLLER: new RegisterUserCommand(...)  →  handler.Handle(command)
                                       │
                                       ▼
                              HANDLER returns Result<RegisterUserResponse>
                                       │
                                       ▼
                    CONTROLLER: Result → IActionResult (Created / Problem)
                                       │
                                       ▼
                                 MVC: serialize → JSON + status code
                                       │
CLIENT ◀────────────────────────── HTTP response
```

---

## Mental model

> The DI container **builds your objects**; the MVC pipeline **moves data in
> and out**. Request in → route → DI constructs the controller + handler tree →
> bind JSON to a request → your code maps it to a command and calls the handler
> → handler returns a `Result` → your code maps it to an `IActionResult` →
> serialize back to JSON.

Construction is **inverted**: you declare what you need in a constructor and the
container fulfills it. You never call the handler's constructor yourself — that
is Dependency Injection doing its job.
