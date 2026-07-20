namespace GameHub.Application.Users.RegisterUser;

public sealed record RegisterUserCommand(
    string Username,
    string Email,
    string Password);
