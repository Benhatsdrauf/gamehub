using GameHub.Application.Common.Messaging;
using GameHub.Application.Common.Results;

namespace GameHub.Application.Authentication.Login;

public sealed record LoginCommand(
    string Email,
    string Password) : ICommand<Result<LoginResponse>>;
