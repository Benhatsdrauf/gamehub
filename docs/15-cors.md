# CORS (Note)

Why the API needs a CORS policy the moment a browser (our SPA) starts calling it —
and why none of our curl/Postman tests ever did.

---

## The idea

CORS exists because of one browser rule: the **Same-Origin Policy**. A browser will
not let JavaScript from one **origin** read responses from a *different* origin, unless
the server explicitly allows it.

An **origin** is `scheme + host + port`. So:

```
http://localhost:5173   (the SPA)   ─┐  different PORT →
http://localhost:5206   (the API)   ─┘  different ORIGIN
```

By default the browser blocks the SPA's `fetch` to the API from *reading the response*.

**Why the rule exists:** without it, any site you visit could silently call
`yourbank.com/api` using your logged-in session and read the result. The Same-Origin
Policy is a core browser security boundary.

**CORS (Cross-Origin Resource Sharing)** is how the **server opts specific origins in**:
it returns headers like `Access-Control-Allow-Origin`, and the browser, seeing them,
allows the calling page to read the response.

---

## The one mental model to keep

> **CORS is enforced by the _browser_ and granted by the _server_. It is not a firewall.**

It does **not** stop a request from reaching your API. It stops the *browser* from
handing the response back to the calling page unless the server allow-listed that origin.

Consequences:
- **Non-browser clients ignore CORS entirely.** curl, Postman, another backend, a mobile
  app — none enforce it. That is why every curl/Postman test in this project worked with
  no CORS config; CORS only bites once a browser is in the loop.
- CORS is **not** authentication or authorization. A wide-open CORS policy doesn't
  bypass `[Authorize]`; a strict one doesn't replace it. They're orthogonal.

---

## Preflight (the automatic OPTIONS request)

For "simple" requests the browser just adds an `Origin` header and checks the response.
But for **non-simple** requests — a custom header like `Authorization`, a method like
`PUT`/`DELETE`, or certain content types — the browser first sends an automatic
**preflight**: an `OPTIONS` request asking *"may origin X use method Y with header Z?"*

The server must answer the preflight with the allow headers **before** the real request
is sent. Our requests are all non-simple (they carry a bearer token), so every one is
preceded by a preflight.

This is why the middleware order matters:

```csharp
app.UseCors(SpaCorsPolicy);   // answer preflight + attach headers FIRST
app.UseAuthentication();      // preflight carries no token, so CORS must run before auth
app.UseAuthorization();
```

If `UseCors` ran after auth, the tokenless `OPTIONS` preflight could be rejected before
the CORS headers were ever attached, and the browser would block the real call.

---

## What we configured

```csharp
// Program.cs
const string SpaCorsPolicy = "GameHubSpa";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(SpaCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins)   // exact origins, from config
            .AllowAnyHeader()
            .AllowAnyMethod());
});
```

- **Origins come from config** (`Cors:AllowedOrigins`), set to `http://localhost:5173`
  in `appsettings.Development.json`. Production sets the real frontend origin(s).
- **No `AllowCredentials`.** We send the token in the `Authorization` header, not a
  cookie, so we don't need cookie credentials. (Also, `AllowAnyOrigin` + credentials is
  forbidden by the spec — allow-listing exact origins is required if credentials are ever
  added, e.g. an httpOnly refresh cookie later.)
- **`WithOrigins`, not `AllowAnyOrigin`.** Allow-list the known SPA origin rather than
  opening the API to every site.

---

## Gotchas

- **A CORS error in the browser console is not a server 500.** The request often reaches
  the server and succeeds there; the browser just refuses to expose the response. Check
  the failing request's `Origin` against the allow-list.
- **Ports matter.** `localhost:5173` and `localhost:5174` are different origins. If Vite
  picks a different port, add it (or configure Vite's port).
- **`http` vs `https`** are different origins too.

See `10-authentication-jwt.md` for how the bearer token (the thing that makes our
requests "non-simple") is validated.
