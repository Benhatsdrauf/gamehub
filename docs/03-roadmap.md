# GameHub Development Roadmap

## Overview

GameHub will be developed iteratively using an agile-inspired approach.

Rather than building every feature at once, the project will evolve through a series of milestones. Each milestone introduces new functionality while improving the overall architecture and code quality.

The roadmap is intended to be a living document and will evolve alongside the project.

---

# Phase 1 - Planning

## Status

✅ Complete

### Goals

- Create repository
- Define project vision
- Define requirements
- Create architecture documentation
- Plan database
- Plan API
- Define Git workflow

---

# Phase 2 - Backend Foundation

## Status

🔜 Upcoming

### Goals

- Create .NET Solution
- Configure Clean Architecture
- Configure Dependency Injection
- Configure Entity Framework Core
- Configure PostgreSQL
- Configure Swagger
- Configure Docker

---

# Phase 3 - Authentication

## Status

⏳ Planned

### Features

- User Registration
- Login
- JWT Authentication
- Refresh Tokens
- Role Authorization

---

# Phase 4 - Game Catalog

## Status

⏳ Planned

### Features

- Browse Games
- Search
- Filtering
- Sorting
- Pagination
- Game Details

---

# Phase 5 - Reviews

## Status

⏳ Planned

### Features

- Create Reviews
- Edit Reviews
- Delete Reviews
- Like Reviews
- Report Reviews

---

# Phase 6 - Library & Purchases

## Status

⏳ Planned

### Features

- Purchase Games
- Library
- Favorites
- Purchase History

---

# Phase 7 - Wishlist

## Status

⏳ Planned

### Features

- Add to Wishlist
- Remove from Wishlist
- Sale Notifications

---

# Phase 8 - Friends & Social

## Status

⏳ Planned

### Features

- Friend Requests
- Friends List
- Public Profiles

---

# Phase 9 - Developer Portal

## Status

⏳ Planned

### Features

- Developer Profiles
- Submit Games
- Upload Screenshots
- Patch Notes
- Developer Dashboard
- Analytics

---

# Phase 10 - Administration

## Status

⏳ Planned

### Features

- Approve Games
- Manage Users
- Moderate Reviews
- Discounts

---

# Phase 11 - Design Patterns

## Status

⏳ Planned

Patterns will be introduced only when they solve a real problem.

| Pattern | Usage |
|----------|-------|
| Repository | Data Access |
| Factory | Notification Creation |
| Strategy | Discount Engine |
| Observer | Notifications & Achievements |
| Builder | Complex Game Creation |
| Adapter | External APIs |
| Decorator | Logging & Caching |
| Mediator | Command/query dispatch + validation & logging pipeline |

### Milestone: In-house Mediator

Controllers currently call handlers directly. MediatR was deliberately **not**
adopted — it is now a paid dependency for newer versions, and direct calls keep
the wiring explicit while there is only one handler.

**Trigger:** when the 3rd–4th handler repeats the same cross-cutting boilerplate
(validation, logging) at the top of `Handle`, extract a minimal in-house mediator
instead of copy-pasting.

**Plan (~40–60 lines, no paid dependency):**

1. Introduce `ICommandHandler<TCommand, TResult>` so handlers share a shape.
2. Add a small `Dispatcher` that resolves a command's handler from DI.
3. Add pipeline behaviors (e.g. a `ValidationBehavior`) that wrap the handler.
4. Move the current in-handler validation block into `ValidationBehavior`,
   thinning every handler.

Free alternative if we choose not to build our own: `martinothamar/Mediator`
(MIT, source-generator based). Verify licensing before adopting anything.

---

# Phase 12 - Frontend

## Status

⏳ Planned

### Features

- Authentication
- Game Catalog
- Library
- Wishlist
- Reviews
- Developer Dashboard
- Admin Dashboard

---

# Phase 13 - Testing

## Status

⏳ Planned

### Goals

- Unit Tests
- Integration Tests
- API Testing

---

# Phase 14 - DevOps

## Status

⏳ Planned

### Goals

- Docker
- Docker Compose
- GitHub Actions
- CI/CD Pipeline

---

# Phase 15 - Polish

## Status

⏳ Planned

### Goals

- Logging
- Global Exception Handling
- Rate Limiting
- API Versioning
- Health Checks
- Performance Improvements

---

# Stretch Goals

These features may be implemented after the MVP is complete.

- Steam Integration
- Cloud Saves
- SignalR Real-Time Notifications
- Redis Caching
- Email Verification
- Two-Factor Authentication
- AI-powered Search
- Mobile Application

---

# Definition of Done

A feature is considered complete when:

- Functionality is implemented
- Tests are passing
- Documentation is updated
- Code has been reviewed
- Feature is merged into the main branch