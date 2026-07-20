namespace GameHub.Application.Users.UpdateUser;

public sealed record UpdateUserCommand(
    Guid Id,
    string Username,
    string Email);
