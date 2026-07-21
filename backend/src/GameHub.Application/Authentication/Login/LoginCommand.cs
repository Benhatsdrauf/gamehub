namespace GameHub.Application.Authentication.Login;

public sealed record LoginCommand(
    string Email,
    string Password);
