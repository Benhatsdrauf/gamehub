namespace GameHub.Application.Users.UpdateUser;

public sealed record UpdateUserResponse(
    Guid Id,
    string Username,
    string Email);
