namespace GameHub.API.Contracts.Users;

public sealed record UpdateUserRequest(
    string Username,
    string Email);
