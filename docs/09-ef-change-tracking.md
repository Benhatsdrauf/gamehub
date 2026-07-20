# EF Core Change Tracking (Note)

How updates persist without any hand-written `UPDATE` SQL.

---

## The idea

When you load an **entity** (a real mapped type, no projection), the `DbContext`
keeps two things:

1. a **reference** to the object it gave you, and
2. a **snapshot** of its original property values at load time.

You then mutate the object in memory. Nothing hits the database yet. When you call
`SaveChanges`, EF compares each tracked entity's *current* values against its
*snapshot*, works out what changed, and generates the **minimal** SQL — only the
changed columns.

```csharp
var user = await _users.GetByIdAsync(id);   // loaded + snapshotted
user.UpdateEmail("new@x.com");              // in-memory change only
await _users.UpdateAsync(user);             // SaveChanges → UPDATE Users SET Email = ... WHERE Id = ...
```

The `DbContext` is like a document editor: it remembers what the document looked
like when you opened it, so on "save" it knows exactly what you edited.

---

## Tracked vs untracked

| Load style | Tracked? | Use |
|------------|----------|-----|
| `Users.FirstOrDefaultAsync(...)` (entity) | **Yes** — snapshot taken | Writes: load → mutate → save |
| `Users.Select(u => new Dto(...))` (projection) | **No** — not an entity | Reads: cheaper, and you cannot save a DTO |
| `Users.AsNoTracking()...` | **No** — explicitly off | Read-only entity reads where you want no snapshot |

Entity states the tracker assigns: `Unchanged`, `Modified`, `Added`, `Deleted`.

---

## Why it matters here

- **Updates "just work":** load the tracked entity, call a domain method to change
  it, `SaveChanges`. No explicit `UPDATE`. Because it is already tracked, you do
  **not** call `Update()` — that would force-mark every column modified.
- **Reads project to DTOs** precisely to avoid the tracking cost, and because a
  projection cannot be saved (nothing to leak, nothing to persist).
- **A `DbContext` holds mutable tracking state**, so it is registered **Scoped**
  (one per request) and is not thread-safe — the reason a Singleton repository
  holding one would be a bug (captive dependency).

See `06-request-lifecycle-and-di.md` for lifetimes and `05-vertical-slice-recipe.md`
for where load/mutate/save sits in a command handler.
