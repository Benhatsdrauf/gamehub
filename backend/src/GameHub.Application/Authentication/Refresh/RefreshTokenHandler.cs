using GameHub.Application.Abstractions.Security;
using GameHub.Application.Common.Messaging;
using GameHub.Application.Common.Results;
using GameHub.Application.Users;

namespace GameHub.Application.Authentication.Refresh;

public sealed class RefreshTokenHandler
    : ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IUserRepository _users;

    public RefreshTokenHandler(
        IRefreshTokenRepository refreshTokens,
        IRefreshTokenGenerator refreshTokenGenerator,
        IJwtTokenGenerator tokenGenerator,
        IUserRepository users)
    {
        _refreshTokens = refreshTokens;
        _refreshTokenGenerator = refreshTokenGenerator;
        _tokenGenerator = tokenGenerator;
        _users = users;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // We stored only the hash, so hash the incoming token to look it up.
        var tokenHash = _refreshTokenGenerator.Hash(command.RefreshToken);
        var stored = await _refreshTokens.GetByHashAsync(tokenHash, cancellationToken);

        if (stored is null)
            return Result.Failure<RefreshTokenResponse>(AuthErrors.InvalidRefreshToken);

        if (!stored.IsActive(now))
        {
            // Reuse detection: a token that was already ROTATED is being replayed —
            // a theft signal. Revoke every live token for the user (log out everywhere).
            if (stored.WasRotated)
            {
                var liveTokens = await _refreshTokens.GetNonRevokedForUserAsync(stored.UserId, cancellationToken);
                foreach (var token in liveTokens)
                    token.Revoke(now);

                await _refreshTokens.SaveChangesAsync(cancellationToken);
            }

            return Result.Failure<RefreshTokenResponse>(AuthErrors.InvalidRefreshToken);
        }

        var user = await _users.GetByIdAsync(stored.UserId, cancellationToken);
        if (user is null)
            // The user was deleted but a token lingered — treat as invalid.
            return Result.Failure<RefreshTokenResponse>(AuthErrors.InvalidRefreshToken);

        // Rotate: mint a new pair, revoke the old token, and link it to its successor
        // (that link is what makes a later replay of the old token detectable).
        var accessToken = _tokenGenerator.GenerateAccessToken(user);
        var newRefreshToken = _refreshTokenGenerator.Create(user.Id);

        _refreshTokens.Add(newRefreshToken.Token);
        stored.Revoke(now, newRefreshToken.Token.Id);

        // One SaveChanges commits the revoke-old + add-new together.
        await _refreshTokens.SaveChangesAsync(cancellationToken);

        var response = new RefreshTokenResponse(
            accessToken.Value,
            accessToken.ExpiresAtUtc,
            newRefreshToken.RawToken,
            newRefreshToken.Token.ExpiresAtUtc);

        return Result.Success(response);
    }
}
