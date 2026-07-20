# Handling Vulnerable Dependencies

A repeatable procedure for when a build warning (`NU1903` / `NU1902`) or a
`dotnet list package --vulnerable` report flags a package.

Worked example: `Microsoft.OpenApi 2.0.0` (High, GHSA-v5pm-xwqc-g5wc), pulled in
transitively by `Microsoft.AspNetCore.OpenApi`.

---

## The 4 steps

### 1. See it — what is vulnerable, and how is it referenced?

```bash
dotnet list package --vulnerable --include-transitive
```

Read the two things that decide the fix:

- **Severity** (Low / Moderate / High / Critical).
- **Direct vs Transitive.** A *Direct* package is in your `.csproj`. A
  *Transitive* one was dragged in by another package — you did not ask for it.

```
Transitive Package     Resolved   Severity   Advisory URL
> Microsoft.OpenApi    2.0.0      High       https://github.com/advisories/GHSA-v5pm-xwqc-g5wc
```

### 2. Read the advisory — is it real for us, and what's the fix version?

Open the advisory URL and find:

- **What the vulnerability is** (e.g. circular schema references → stack
  overflow → process crash / DoS).
- **Does it apply to how we use the package?** (Our app *generates* OpenAPI, it
  does not *parse untrusted* OpenAPI, so practical risk was low.) Patch anyway —
  a known-High warning you learn to ignore is worse than the fix.
- **Affected vs Patched versions.** Note the first patched version — the floor
  you must reach. (Affected `2.0.0`–`2.7.4`; patched **`2.7.5`**.)

### 3. Apply the fix — depends on direct vs transitive

**Direct dependency** — just raise its version:

```bash
dotnet add <project> package <Name> --version <patched>
```

**Transitive dependency** — you cannot edit the parent package's dependency
list. Add a **direct** `PackageReference` to the patched version yourself:

```bash
dotnet add src/GameHub.API/GameHub.API.csproj package Microsoft.OpenApi --version 2.7.5
```

Why this works: **NuGet resolves a direct reference over a transitive one.**
Your explicit pin overrides the version the parent dragged in. Stay within the
same major version so it remains API-compatible.

### 4. Verify — prove it's gone

```bash
dotnet build                                        # NU1903 warning should be gone
dotnet list package --vulnerable --include-transitive   # "no vulnerable packages"
```

Do not trust the fix until both are clean.

---

## Notes

- A transitive pin is a **temporary override**. When the parent package later
  updates to depend on a patched version, remove the manual pin so you are not
  holding an old version longer than needed. Re-check periodically.
- Keep the fix as its own `chore:` commit so the security change is easy to find
  in history.
- `--include-transitive` matters — the default view hides transitive packages,
  which is exactly where these often hide.
```

<!--
Worked example result: GameHub.API pinned Microsoft.OpenApi 2.7.5; all projects
report no vulnerable packages.
-->
