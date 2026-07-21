# In-House Mediator (Note)

How a controller reaches a handler without knowing which one — and where validation
now lives.

---

## The idea

Before: a controller injected each concrete handler it needed (`UsersController` held
five). It was coupled to every handler, and each handler re-ran the same validation block.

After: a controller injects **one** dispatcher, `ISender`, and says "handle this request."
The mediator finds the matching handler, runs the request through a **pipeline** of
behaviors, and returns the result. Two payoffs:

1. **Decoupling** — controllers depend on one abstraction, not N handlers.
2. **One home for cross-cutting concerns** — validation (and later logging, timing,
   transactions) lives in a *behavior* that wraps every handler, instead of being
   copy-pasted into each one.

This is a hand-built ~100-line version of MediatR (now paid) / `@nestjs/cqrs`:
`ISender` ≈ `CommandBus`, our handlers ≈ `@CommandHandler`, and `IPipelineBehavior`
≈ a Nest interceptor/pipe.

---

## The pieces (`Application/Common/Messaging/`)

| Type | Role |
|------|------|
| `IRequest<TResponse>` | marker: "handling me yields a `TResponse`" |
| `ICommand<T>` / `IQuery<T>` | semantic markers over `IRequest` — write vs read (CQRS) |
| `IRequestHandler<TRequest, TResponse>` | `Handle(request, ct)` — the logic |
| `ICommandHandler` / `IQueryHandler` | thin aliases so a handler declares its side |
| `ISender` | the dispatcher: `Send(request)` |
| `IPipelineBehavior<TRequest, TResponse>` | wraps handling: `Handle(request, next, ct)` |
| `RequestHandlerDelegate<TResponse>` | "the next step in the chain" |
| `Sender` | the dispatcher implementation |
| `ValidationBehavior` | the one behavior we ship: runs FluentValidation |

---

## How a request flows

```
controller: _sender.Send(new RegisterUserCommand(...))
   │
   ▼  Sender.Send<TResponse>(IRequest<TResponse> request)
   │   request.GetType() = RegisterUserCommand   ← runtime type is the bridge
   ▼
resolve IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>> from DI
   ▼
wrap handler in behaviors (reversed → registration order = execution order)
   ▼
run:  ValidationBehavior → … → RegisterUserHandler
```

### The dispatch (the one place with reflection)

`Send` only sees the request as `IRequest<TResponse>` at compile time. To find the
handler it needs the **concrete** type, which it gets from `request.GetType()`. It
then builds the closed handler interface (`IRequestHandler<RegisterUserCommand, …>`)
with `MakeGenericType`, resolves it from DI, and invokes `Handle`. It wraps that in
the resolved behaviors, innermost-first.

Because it invokes via reflection, any exception a handler throws arrives wrapped in
`TargetInvocationException` — the `Sender` unwraps it (`ExceptionDispatchInfo`) so
callers see the original exception, exactly as if they'd called the handler directly.
Reflection stays invisible outside this file.

---

## Where validation lives now

`ValidationBehavior<TRequest, TResponse>` is the single home for FluentValidation.
For each request it:

1. gets all `IValidator<TRequest>` (FluentValidation registers them);
2. if there are none (e.g. a simple query), calls `next()` and returns;
3. runs them; if all pass, calls `next()` (the handler runs);
4. if any fail, **short-circuits** — it builds the same `ValidationError` map the API
   expects and returns a *failed* result. **The handler never runs.**

The handlers no longer contain any validation code — the ~10-line block that used to
be duplicated in Register, Update, and Login is gone.

### Where the validators come from

The behavior contains **no rules**. Each command that needs validation still has its
own validator class — the rules did not move or centralize; only the code that *runs*
them did. Three links connect a validator to the behavior, all via DI:

1. **The validator declares its command.**
   `RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>` — one per
   command that needs it (Register, Update, Login today). These are the FluentValidation
   equivalents of a Zod schema per input shape.
2. **The assembly scan registers them all.** `AddValidatorsFromAssembly(...)` registers
   every `AbstractValidator<T>` under `IValidator<T>`, e.g.
   `IValidator<RegisterUserCommand>` → `RegisterUserCommandValidator`.
3. **The behavior asks DI for the validators of *this* request.** Its constructor takes
   `IEnumerable<IValidator<TRequest>>`. When the pipeline runs for a `RegisterUserCommand`,
   `TRequest` is `RegisterUserCommand`, so DI injects exactly that command's validator(s).
   The behavior never names a specific validator.

```
Send(RegisterUserCommand) → IEnumerable<IValidator<RegisterUserCommand>> = [RegisterUserCommandValidator] → runs it
Send(GetUserQuery)        → IEnumerable<IValidator<GetUserQuery>>        = []  → nothing to run → straight to handler
```

So queries with no validator class (GetUser, GetUsers, Delete) flow straight through.
Add a validator class later and it is picked up automatically — nothing else changes.

### The one subtlety: building a failure generically

The behavior is generic over `TResponse`, but a failure has to be a `Result`. We
constrain `where TResponse : Result` (true for every handler). At runtime:

- if `TResponse` is exactly `Result` (e.g. DeleteUser) → `Result.Failure(error)`;
- otherwise it's `Result<T>` → pull out `T` and call `Result.Failure<T>(error)` via
  reflection.

That is the only spot that can't stay compile-time typed, and it's one small helper.

---

## Registration (`AddApplication`)

- `AddValidatorsFromAssembly` — registers every validator (consumed by the behavior).
- `AddScoped<ISender, Sender>()` — **Scoped**, so the `IServiceProvider` it captures is
  the request scope and can resolve scoped handlers (which depend on the scoped
  `DbContext`). A singleton here would be a captive-dependency bug.
- `AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))` — the
  open-generic behavior; more behaviors register here, first = outermost.
- `AddRequestHandlers()` — scans the assembly for `IRequestHandler<,>` implementations
  and registers each under its closed interface. **Adding a handler never touches DI again.**

---

## Adding a slice now

1. A command/query: `record FooCommand(...) : ICommand<Result<FooResponse>>`.
2. A handler: `class FooHandler : ICommandHandler<FooCommand, Result<FooResponse>>`.
3. (Optional) a `FooCommandValidator : AbstractValidator<FooCommand>`.
4. A controller action: `await _sender.Send(command)`.

No DI edits, and validation is automatic. See `05-vertical-slice-recipe.md` for the
full slice shape and `06-request-lifecycle-and-di.md` for service lifetimes.
