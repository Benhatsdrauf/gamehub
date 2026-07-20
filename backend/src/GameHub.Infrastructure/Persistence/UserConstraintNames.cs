namespace GameHub.Infrastructure.Persistence;

// Single source of truth for the User unique-index names. Referenced by both
// the EF configuration (which creates them) and the repository (which detects
// violations by name), so the two can never drift apart.
public static class UserConstraintNames
{
    public const string UniqueEmail = "IX_Users_Email";

    public const string UniqueUsername = "IX_Users_Username";
}
