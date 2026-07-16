# Architecture

## Overview

GameHub is designed as a modern full-stack application following the principles of Clean Architecture.

The application is built as a modular monolith. While all functionality is deployed as a single application, each feature is organized into independent modules with clearly defined responsibilities. This approach provides the simplicity of a monolithic application while maintaining a clean separation of concerns and allowing future expansion.

The architecture emphasizes maintainability, scalability, and testability over unnecessary complexity.

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

The project follows several core principles:

- Clean Architecture
- SOLID Principles
- Separation of Concerns
- Dependency Injection
- Domain-Driven Design (lightweight)
- Test-Driven mindset where appropriate

---

# Project Structure

```
backend/

src/
│
├── GameHub.API
├── GameHub.Application
├── GameHub.Domain
└── GameHub.Infrastructure

tests/
│
├── GameHub.UnitTests
└── GameHub.IntegrationTests
```

---

# Layer Responsibilities

## GameHub.API

Responsible for:

- REST Controllers
- Authentication
- Authorization
- Middleware
- Dependency Injection
- Swagger
- API Versioning

---

## GameHub.Application

Responsible for:

- Business Logic
- DTOs
- Interfaces
- Validation
- Use Cases
- Services

---

## GameHub.Domain

Responsible for:

- Entities
- Enums
- Domain Rules
- Value Objects
- Domain Events

The Domain layer contains the business logic and must not depend on any external framework.

---

## GameHub.Infrastructure

Responsible for:

- Entity Framework Core
- PostgreSQL
- Repository Implementations
- Authentication Providers
- Logging
- File Storage
- External Services

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

The Domain layer never references Infrastructure or API.

---

# Technology Stack

## Backend

- .NET 10
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- FluentValidation
- AutoMapper (where appropriate)
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
| Observer | Notifications and achievements |
| Builder | Complex game creation |
| Adapter | External game integrations |
| Decorator | Logging and caching |
| Dependency Injection | Service composition |

Patterns will only be introduced when they solve an actual problem within the application.

---

# User Roles

## Player

- Purchase games
- Leave reviews
- Manage library
- Add friends
- Unlock achievements
- Maintain wishlist

---

## Developer

- Create developer profile
- Submit games
- Update games
- Publish patch notes
- Upload screenshots
- View analytics

---

## Administrator

- Approve game submissions
- Moderate reviews
- Manage users
- Manage discounts
- Suspend developers

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

The primary goals of this architecture are:

- Maintainability
- Scalability
- Testability
- Readability
- Flexibility
- Professional software engineering practices

The architecture should allow the application to grow without requiring significant restructuring while remaining approachable for new contributors.