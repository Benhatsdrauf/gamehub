namespace GameHub.Application.Authentication.Login;

public sealed record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    Guid UserId,
    string Username,
    string Email,
    string Role);
