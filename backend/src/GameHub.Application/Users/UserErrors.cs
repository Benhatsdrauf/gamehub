using GameHub.Application.Common.Errors;

namespace GameHub.Application.Users;

public static class UserErrors
{
    public static Error EmailNotUnique(string email) => new(
        "User.EmailNotUnique",
        $"A user with the email '{email}' already exists.",
        ErrorType.Conflict);
}
