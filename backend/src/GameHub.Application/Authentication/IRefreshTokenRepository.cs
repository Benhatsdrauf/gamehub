using GameHub.Domain.Authentication;

namespace GameHub.Application.Authentication;

public interface IRefreshTokenRepository
{
    // Stages a new token for insertion. Does NOT save — the caller controls the unit
    // of work so rotation (revoke old + add new) commits in a single SaveChanges.
    void Add(RefreshToken token);

    // Looks a token up by its hash (tracked, so mutations persist on SaveChanges).
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    // Every not-yet-revoked token for a user — used to revoke a whole session on
    // reuse detection.
    Task<IReadOnlyList<RefreshToken>> GetNonRevokedForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
