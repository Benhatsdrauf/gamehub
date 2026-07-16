# Functional Requirements

## User Management

### FR-001
Users can register for an account.

### FR-002
Users can log in using their email and password.

### FR-003
Users can edit their profile information.

### FR-004
Users can upload or change their avatar.

### FR-005
Users can change their password.

---

# Game Catalog

### FR-006
Users can browse all published games.

### FR-007
Users can search games by title.

### FR-008
Users can filter games by:

- Genre
- Platform
- Price
- Rating
- Developer

### FR-009
Users can sort games by:

- Release Date
- Price
- Rating
- Popularity

### FR-010
Users can view detailed information about a game.

---

# Reviews

### FR-011
Authenticated users can leave reviews.

### FR-012
Users can edit their own reviews.

### FR-013
Users can delete their own reviews.

### FR-014
Users can like helpful reviews.

### FR-015
Users can report inappropriate reviews.

---

# Purchases

### FR-016
Users can purchase games.

### FR-017
Purchased games appear in the user's library.

### FR-018
Users can view their purchase history.

---

# Library

### FR-019
Users can view all owned games.

### FR-020
Users can mark owned games as favorites.

---

# Wishlist

### FR-021
Users can add games to their wishlist.

### FR-022
Users can remove games from their wishlist.

### FR-023
Users receive a notification when a wishlisted game goes on sale.

---

# Friends & Social

### FR-024
Users can send friend requests.

### FR-025
Users can accept or reject friend requests.

### FR-026
Users can remove friends.

### FR-027
Users can view another user's public profile.

---

# Notifications

### FR-028
Users receive notifications when:

- A friend request is received.
- A purchase is completed.
- A review receives a like.
- A wishlisted game goes on sale.
- A published game receives an update.
- An achievement is unlocked.

---

# Achievements

### FR-029
Users can unlock achievements by completing milestones.

Example achievements:

- First Purchase
- First Review
- Five Reviews Written
- Own 10 Games
- Own 25 Games
- Own 50 Games
- Wishlist Creator
- Social Gamer

### FR-030
Users can view unlocked achievements.

### FR-031
Users can track progress toward locked achievements.

---

# Developer Portal

### FR-032
Users can create a Developer Profile.

### FR-033
Developers can submit new games for review.

### FR-034
Developers can edit their own games.

### FR-035
Developers can upload game cover images and screenshots.

### FR-036
Developers can publish updates and patch notes.

### FR-037
Developers can view analytics for their published games.

---

# Administration

### FR-038
Administrators can approve or reject submitted games.

### FR-039
Administrators can manage users.

### FR-040
Administrators can moderate reported reviews.

### FR-041
Administrators can manage discounts.

### FR-042
Administrators can suspend developer accounts.

---

# Non-Functional Requirements

## Performance

- API responses should typically complete within 500 ms.
- Search and filtering should remain responsive for large datasets.

## Security

- JWT Authentication
- Password hashing
- Role-based authorization
- Input validation

## Reliability

- Global exception handling
- Structured logging
- Meaningful error responses

## Maintainability

- Clean Architecture
- SOLID Principles
- Design Patterns where appropriate
- Unit Tests
- Integration Tests

## Scalability

- Modular Monolith architecture
- Repository abstraction
- Dependency Injection
- Future migration path to microservices if needed

## Deployment

- Docker
- Docker Compose
- GitHub Actions

## Documentation

- Swagger / OpenAPI
- README
- Architecture Documentation
- Database Documentation