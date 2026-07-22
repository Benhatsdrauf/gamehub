using GameHub.Domain.Authentication;

namespace GameHub.Application.Abstractions.Security;

// A freshly minted refresh token: the entity to persist (hash + expiry already set)
// and the RAW token string, which is returned to the client once and never stored.
public sealed record NewRefreshToken(RefreshToken Token, string RawToken);
