using GameHub.Application.Common.Errors;

namespace GameHub.Application.Users;

public static class UserErrors
{
    public static Error EmailNotUnique(string email) => new(
        "User.EmailNotUnique",
        $"A user with the email '{email}' already exists.",
        ErrorType.Conflict);

    public static Error UsernameNotUnique(string username) => new(
        "User.UsernameNotUnique",
        $"A user with the username '{username}' already exists.",
        ErrorType.Conflict);

    public static Error NotFound(Guid id) => new(
        "User.NotFound",
        $"No user with the ID '{id}' was found.",
        ErrorType.NotFound);
}
