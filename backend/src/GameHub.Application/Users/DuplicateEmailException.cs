namespace GameHub.Application.Users;

// Thrown by the persistence layer when a unique-email violation is detected at
// the database (the race-loser case). It is an Application-owned type, so the
// EF Core DbUpdateException never escapes Infrastructure.
public sealed class DuplicateEmailException : Exception
{
    public DuplicateEmailException()
        : base("A user with this email already exists.")
    {
    }
}
