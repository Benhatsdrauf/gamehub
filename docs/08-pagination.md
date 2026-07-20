# Pagination

A list endpoint must **never** return an unbounded result set. `SELECT *` over a
large table loads every row into memory, serializes it all to JSON, and pushes it
over the wire — an out-of-memory crash or timeout waiting to happen. Every
collection endpoint returns a bounded **page**.

There are two ways to slice a result set. They are not interchangeable — pick by
the endpoint's access pattern.

---

## Offset pagination

Client sends a page number and size:

```
GET /api/users?page=3&pageSize=20
```

Translates to SQL:

```sql
SELECT ... ORDER BY Username LIMIT 20 OFFSET 40
```

**Pros**
- Simple and universally understood.
- Supports "jump to page 7" and showing total pages (`Page 3 of 47`).
- Easy total count.

**Cons**
- Slow at deep offsets: the database must scan and skip every preceding row
  (`OFFSET 1_000_000` walks a million rows before returning any).
- Unstable under concurrent inserts/deletes: rows shift between pages, so an item
  can be seen twice or skipped when paging while data changes.

**Best for:** admin/moderation tables, dashboards — bounded data where users want
page numbers and totals.

---

## Keyset (cursor) pagination

Client sends a cursor pointing after the last item it saw:

```
GET /api/games?after=<lastId>&pageSize=20
```

Translates to SQL:

```sql
SELECT ... WHERE Id > @after ORDER BY Id LIMIT 20
```

**Pros**
- Fast at any depth: it *seeks* via an index instead of scanning-and-skipping.
- Stable under inserts/deletes: the cursor anchors to a real row.

**Cons**
- No "jump to page N" and no easy total count.
- The client must carry the cursor forward.
- Slightly more complex to implement.

**Best for:** infinite scroll, very large datasets, public feeds — where depth and
stability matter more than page numbers.

---

## What GameHub uses

**Offset pagination** for list endpoints, starting with `GET /api/users`.

**Why:** the user list is an admin/moderation screen. Admins want page numbers, a
total count, and the ability to jump around — exactly offset's strengths. Its
weaknesses (deep-offset cost, drift under churn) do not bite at admin-scale data.

We will reach for **keyset** later on a genuinely different access pattern — e.g.
a public infinite-scroll game feed — where it earns its extra complexity. The
principle: **match the pagination style to the access pattern; do not default to
the fancier tool everywhere.**

### Response envelope

List endpoints return `PagedResponse<T>`, not a bare array, so the client can
render "Page 3 of 47":

```jsonc
{
  "items": [ /* the page of DTOs */ ],
  "page": 3,
  "pageSize": 20,
  "totalCount": 934,
  "totalPages": 47
}
```

### Guardrails

Client paging input is never trusted. The handler clamps it:

| Parameter  | Rule                                    |
|------------|-----------------------------------------|
| `page`     | `< 1` → `1`                             |
| `pageSize` | `< 1` → default (20); `> 100` → max (100) |

This stops `?pageSize=1000000` from defeating the entire point of paging. Clamping
(rather than returning `400`) is intentional for pagination: an over-large request
is served the maximum allowed page rather than rejected.

Implementation: `GetUsersHandler` (clamping) + `UserQueries.GetUsersAsync`
(the `COUNT` + `ORDER BY / Skip / Take` projection). Note the `COUNT` is a second
query needed to compute `totalPages`.
