namespace GameHub.Application.Authentication.Refresh;

// A fresh token pair. The client replaces both of its stored tokens with these — the
// old refresh token is now revoked and must not be reused.
public sealed record RefreshTokenResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
