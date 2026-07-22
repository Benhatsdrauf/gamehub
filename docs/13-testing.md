# Testing (Note)

How we test GameHub, why the stack is what it is, and what each kind of test can and
cannot catch.

---

## The idea

Two layers, each answering a different question:

- **Unit tests** — "does *this class's logic* behave?" Fast (milliseconds), no database,
  no HTTP. Dependencies are faked. Where handlers, validators, and small services are proven.
- **Integration tests** — "does the *real wiring* work?" A real HTTP request through the
  whole stack: routing, `[Authorize]`, the mediator, the validation behavior, EF, Postgres.
  Slower, needs a throwaway database, but catches everything mocks can't.

Rule of thumb: **most tests are unit tests** (cheap, precise), with a **thinner layer of
integration tests** over the paths that matter most (auth, a full slice end to end). Unit
tests find logic bugs; integration tests find wiring bugs. You need both.

Current state: **unit tests started** (the RegisterUser slice). **Integration tests not
built yet** — that is the deliberate next testing layer.

---

## The stack, and why

| Concern | Choice | Why (and what we avoided) |
|---------|--------|---------------------------|
| Test framework | **xUnit** | Standard, already in both test projects. `[Fact]` = one test, `[Theory]` = parameterized. |
| Mocking | **NSubstitute** | Free, clean syntax. Avoided **Moq** (its SponsorLink episode). |
| Assertions | **plain xUnit `Assert`** | Zero dependencies. Avoided **FluentAssertions** — it went **commercial in v8**, the same trap as MediatR. Free alternatives if we ever want fluent syntax: Shouldly, AwesomeAssertions. |

The "avoid the paid/again-controversial default" instinct is the same one that led to the
in-house mediator (see `12-mediator.md`).

---

## Anatomy of a test

```csharp
public class RegisterUserHandlerTests            // groups tests (≈ describe)
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly RegisterUserHandler _sut;   // "system under test"

    public RegisterUserHandlerTests()            // constructor runs before EACH test (≈ beforeEach)
    {
        _sut = new RegisterUserHandler(_users, ...);
    }

    [Fact]                                       // one test (≈ it/test)
    public async Task Handle_Scenario_Expected()
    {
        // Arrange — inputs and fakes
        // Act     — call the one thing under test
        // Assert  — verify
    }
}
```

- **A fresh class instance per test.** xUnit constructs the class anew for every `[Fact]`,
  so no state leaks between tests — isolation is automatic.
- **`[Fact]`** = a single test. **`[Theory]` + `[InlineData(...)]`** = the same test run
  over many inputs (parameterized; like `test.each`).
- **Naming: `Method_Scenario_ExpectedResult`.** The method name is the test description in
  the output — make it read like a sentence.
- **`_sut`** ("system under test") names the object the test actually exercises.
- **Arrange–Act–Assert** is the shape of every body.

---

## Unit-testing a handler (mocking the ports)

Handlers depend only on **interfaces** (`IUserRepository`, `IPasswordHasher`, …). That is
exactly what lets us swap in fakes and test the logic with no database and no real bcrypt —
the payoff of Ports & Adapters.

```csharp
_passwordHasher.Hash("supersecret123").Returns("HASHED");   // stub a return value
var result = await _sut.Handle(new RegisterUserCommand("alice", "alice@example.com", "supersecret123"));

Assert.True(result.IsSuccess);
await _users.Received(1).AddAsync(                            // assert an INTERACTION
    Arg.Is<User>(u => u != null && u.PasswordHash == "HASHED"),
    Arg.Any<CancellationToken>());
```

- **`Substitute.For<T>()`** — a fake implementing the whole interface (≈ `vi.fn()` over an interface).
- **`.Returns(x)`** — stub what a call yields (≈ `mockReturnValue`). An *unconfigured*
  method returns the default — for `Task<bool>` that is `false`, which is why the
  "email is unique" case needs no setup.
- **`.Received(1)` / `.DidNotReceive()`** — assert a method *was* / *was not* called
  (≈ `toHaveBeenCalledTimes` / `not.toHaveBeenCalled`).
- **`Arg.Is<T>(predicate)` / `Arg.Any<T>()`** — argument matchers. Once one argument uses a
  matcher, all of them must. (`u` is null-guarded because a matcher is null-tolerant.)

Two flavors of assertion show up here:
- **State** — assert on the returned value (`result.IsSuccess`, `result.Error.Code`).
- **Behavior** — assert on *how the handler used its dependencies* (persisted the **hashed**
  password; on a duplicate email, wrote nothing and hashed nothing). Testing the *absence*
  of an action is often as important as testing a present one.

---

## Unit-testing a validator

The easiest thing to test — no dependencies:

```csharp
var result = new RegisterUserCommandValidator().Validate(command);
Assert.False(result.IsValid);
Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.Email));
```

Assert on *which field* failed (`nameof` keeps it rename-safe), not the exact message — so
rewording a rule doesn't break the test. (FluentValidation also ships a `TestHelper` with
fluent sugar; we use plain `Validate` + `Assert` to keep one assertion style.)

Note: since the mediator refactor, validation runs in `ValidationBehavior`, not in handlers.
So a handler unit test no longer exercises validation — the validator test covers the rules,
and an integration test covers "does the behavior actually run in the pipeline."

---

## What unit tests cannot catch (→ integration tests)

Mocks prove logic, but a suite of green unit tests can still ship a broken app, because
mocks can't see the real wiring:

- Does `[Authorize]` / `[Authorize(Roles="Admin")]` actually block the request?
- Does the mediator resolve and invoke the right handler, with the behavior in the pipeline?
- Does EF map the entity to Postgres and persist the right columns?
- Does the JWT validate on the way back in?

Those need an **integration test**: `WebApplicationFactory` boots the real app in-memory and
fires real HTTP; a disposable Postgres (via **Testcontainers**, a throwaway DB in Docker)
backs it. Not built yet — the planned next testing layer.

---

## Running the tests

```bash
dotnet test                                   # everything
dotnet test tests/GameHub.UnitTests/…         # one project
```

Tests mirror the source layout (`tests/GameHub.UnitTests/Users/RegisterUser/…`) so a slice's
tests sit where you'd look for them. See `05-vertical-slice-recipe.md` for the slice shape
and `12-mediator.md` for why handlers are now pure logic (and thus easy to unit-test).
