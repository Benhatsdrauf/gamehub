namespace GameHub.API.Contracts.Users;

public sealed record RegisterUserRequest(
    string Username,
    string Email,
    string Password);
