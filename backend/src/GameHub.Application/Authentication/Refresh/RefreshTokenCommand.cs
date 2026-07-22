using GameHub.Application.Common.Messaging;
using GameHub.Application.Common.Results;

namespace GameHub.Application.Authentication.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken)
    : ICommand<Result<RefreshTokenResponse>>;
