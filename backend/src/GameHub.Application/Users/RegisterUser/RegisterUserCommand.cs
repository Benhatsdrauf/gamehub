using GameHub.Application.Common.Messaging;
using GameHub.Application.Common.Results;

namespace GameHub.Application.Users.RegisterUser;

public sealed record RegisterUserCommand(
    string Username,
    string Email,
    string Password) : ICommand<Result<RegisterUserResponse>>;
