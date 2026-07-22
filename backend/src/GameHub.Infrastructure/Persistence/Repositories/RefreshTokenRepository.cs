using GameHub.Application.Authentication;
using GameHub.Domain.Authentication;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly GameHubDbContext _dbContext;

    public RefreshTokenRepository(GameHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Stage only — the caller decides when to SaveChanges (so rotation commits the
    // revoke-old + add-new pair together).
    public void Add(RefreshToken token) => _dbContext.RefreshTokens.Add(token);

    // Tracked (no AsNoTracking): the token may be mutated (Revoke) and persisted.
    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public async Task<IReadOnlyList<RefreshToken>> GetNonRevokedForUserAsync(
        Guid userId, CancellationToken cancellationToken = default) =>
        await _dbContext.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
