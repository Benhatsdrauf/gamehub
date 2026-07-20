namespace GameHub.Application.Users.GetUser;

public sealed record GetUserResponse(
    Guid Id,
    string Username,
    string Email);
