using GameHub.Application.Abstractions.Security;
using GameHub.Application.Common.Messaging;
using GameHub.Application.Common.Results;

namespace GameHub.Application.Authentication.Logout;

public sealed class LogoutHandler
    : ICommandHandler<LogoutCommand, Result>
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    public LogoutHandler(
        IRefreshTokenRepository refreshTokens,
        IRefreshTokenGenerator refreshTokenGenerator)
    {
        _refreshTokens = refreshTokens;
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public async Task<Result> Handle(
        LogoutCommand command,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = _refreshTokenGenerator.Hash(command.RefreshToken);
        var stored = await _refreshTokens.GetByHashAsync(tokenHash, cancellationToken);

        // Idempotent: revoke it if it exists and is still live. Always return success —
        // never reveal whether the token was real (same reasoning as the login errors).
        if (stored is not null && !stored.IsRevoked)
        {
            stored.Revoke(DateTime.UtcNow);
            await _refreshTokens.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
