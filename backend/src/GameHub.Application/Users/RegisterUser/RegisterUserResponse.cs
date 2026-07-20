namespace GameHub.Application.Users.RegisterUser;

public sealed record RegisterUserResponse(
    Guid Id,
    string Username,
    string Email);
