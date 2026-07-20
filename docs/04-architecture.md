# Architecture

## Overview

GameHub is a modern full-stack application built using ASP.NET Core and React.

The backend follows the principles of Clean Architecture while organizing features using Vertical Slice Architecture. The application is implemented as a **Modular Monolith**, providing clear boundaries between modules while remaining simple to develop, test, and deploy.

The architecture prioritizes maintainability, readability, scalability, and testability over unnecessary complexity. Dependencies always point inward, keeping the business domain independent from frameworks and infrastructure.  [oai_citation:0‡Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures?utm_source=chatgpt.com)

---

# High-Level Architecture

```
                React Frontend
                       │
                  HTTPS / REST
                       │
                ASP.NET Core API
                       │
        ┌──────────────┴──────────────┐
        │                             │
 Application Layer          Infrastructure Layer
        │                             │
        └──────────────┬──────────────┘
                       │
                  Domain Layer
                       │
                  PostgreSQL
```

---

# Architectural Principles

GameHub follows these principles:

- Clean Architecture
- SOLID Principles
- Separation of Concerns
- Dependency Injection
- Vertical Slice Architecture
- Domain-Driven Design (lightweight)
- Feature-first organization
- Test-Driven mindset where appropriate

---

# Project Structure

```
backend/

src/

├── GameHub.API
├── GameHub.Application
├── GameHub.Domain
└── GameHub.Infrastructure

tests/

├── GameHub.UnitTests
└── GameHub.IntegrationTests
```

---

# Domain Structure

```
GameHub.Domain

├── Common
├── Models
├── ValueObjects
├── Events
└── Enums
```

Models will contain our core business concepts.

Example:

```
Models/

User

DeveloperProfile

Game

Genre

Review

Purchase

Achievement
```

---

# Layer Responsibilities

## GameHub.API

Responsible for:

- Controllers
- Authentication configuration
- Authorization
- Middleware
- Dependency Injection
- Swagger
- API Versioning

The API should contain no business logic.

---

## GameHub.Application

Responsible for:

- Business workflows
- Commands
- Queries
- DTOs
- Interfaces
- Validation
- Application Services

Features are organized vertically.

Example:

```
Games/

Commands/

Queries/

DTOs/

Validators/
```

---

## GameHub.Domain

Responsible for:

- Business Models
- Enums
- Value Objects
- Domain Events
- Business Rules

The Domain layer has **no dependency** on ASP.NET Core, Entity Framework, PostgreSQL, or any external framework.  [oai_citation:1‡Clean Architecture](https://cleanarchitecture.jasontaylor.dev/docs/architecture/?utm_source=chatgpt.com)

---

## GameHub.Infrastructure

Responsible for:

- Entity Framework Core
- PostgreSQL
- Repository implementations
- Authentication
- Logging
- File Storage
- External APIs
- Email

Infrastructure implements the abstractions defined by the Application layer.

---

# Dependency Rule

Dependencies always point inward.

```
Infrastructure
      │
      ▼
Application
      │
      ▼
Domain
```

The Domain layer never depends on any outer layer.

---

# Pagination

List endpoints never return an unbounded result set — they return a bounded page
wrapped in a `PagedResponse<T>` envelope (`items`, `page`, `pageSize`,
`totalCount`, `totalPages`).

**Chosen strategy: offset pagination** (`?page=&pageSize=`).

**Why:** the current list endpoints (starting with `GET /api/users`) are
admin/moderation screens, where users expect page numbers, a total count, and the
ability to jump to any page — offset's strengths. Its weaknesses (slow deep
offsets, drift under concurrent writes) do not matter at admin-scale data.
Keyset/cursor pagination is reserved for a later, genuinely different access
pattern (e.g. a public infinite-scroll feed).

Paging input is clamped in the handler (`page >= 1`, `pageSize` in `[1, 100]`,
default `20`) so a client cannot request an unbounded page.

See `08-pagination.md` for the full offset-vs-keyset comparison and rationale.

---

# Technology Stack

## Backend

- .NET 10
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- FluentValidation
- Serilog
- xUnit

---

## Frontend

- React
- TypeScript
- Vite
- Tailwind CSS
- TanStack Query
- React Hook Form
- Zod
- Axios

---

## DevOps

- Docker
- Docker Compose
- GitHub Actions

---

# Planned Design Patterns

| Pattern | Planned Usage |
|----------|---------------|
| Repository | Data access |
| Factory | Notification creation |
| Strategy | Discount calculations |
| Observer | Notifications & Achievements |
| Builder | Complex Game creation |
| Adapter | External game integrations |
| Decorator | Logging & Caching |
| Dependency Injection | Service composition |

Patterns will only be introduced when they solve a real problem.

---

# User Model

Every authenticated person is represented by a **User**.

A User can optionally create a **DeveloperProfile**.

Creating a DeveloperProfile grants publishing capabilities while the user still retains all normal user functionality.

```
User
 │
 ├── Library
 ├── Wishlist
 ├── Friends
 ├── Reviews
 └── DeveloperProfile (optional)
           │
           └── Games
```

This models the business domain more accurately than using inheritance or a dedicated Developer role.

---

# User Types

## User

Every authenticated account is a User.

Users can:

- Purchase games
- Maintain a library
- Create reviews
- Add friends
- Unlock achievements
- Maintain a wishlist

Users may optionally create a Developer Profile.

---

## Developer Profile

A Developer Profile extends a User's capabilities.

It contains:

- Display Name
- Biography
- Website
- Logo

Developers can:

- Submit games
- Publish updates
- Upload screenshots
- View analytics

---

## Administrator

Administrators have elevated permissions.

Administrators can:

- Approve or reject games
- Moderate reviews
- Manage users
- Manage discounts
- Suspend developer profiles

---

# Planned Domain Model

```
User
│
├── Library
├── Wishlist
├── Reviews
├── Friends
└── DeveloperProfile (optional)
          │
          └── Games
                  │
                  ├── Genres
                  ├── Platforms
                  ├── Reviews
                  ├── Screenshots
                  └── Achievements
```

---

# Future Enhancements

Potential future features include:

- Steam Integration
- Epic Games Import
- Cloud Saves
- Email Verification
- Two-Factor Authentication
- Redis Caching
- SignalR Notifications
- Admin Analytics Dashboard
- Game Recommendations
- AI-powered Search
- Mobile Application

---

# Architecture Goals

The architecture is designed to provide:

- Maintainability
- Readability
- Scalability
- Testability
- Flexibility
- Professional software engineering practices

As the application evolves, this document will evolve alongside it. It is intended to be a living document rather than static documentation.