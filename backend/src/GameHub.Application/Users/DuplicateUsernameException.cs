namespace GameHub.Application.Users;

// Thrown by the persistence layer when a unique-username violation is detected
// at the database (the race-loser case). Application-owned, so the EF Core
// DbUpdateException never escapes Infrastructure.
public sealed class DuplicateUsernameException : Exception
{
    public DuplicateUsernameException()
        : base("A user with this username already exists.")
    {
    }
}
