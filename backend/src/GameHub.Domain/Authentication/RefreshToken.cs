using GameHub.Domain.Core;

namespace GameHub.Domain.Authentication;

// A stored, revocable credential used to mint fresh access tokens without re-login.
// We persist only the HASH of the token (never the token itself). The entity owns its
// own lifecycle: it can be revoked, and it remembers which token replaced it (rotation)
// — which is what lets us detect a replayed, already-used token (theft).
public sealed class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    // Null while the token is still live; set the moment it is revoked (by rotation,
    // logout, or reuse-detection).
    public DateTime? RevokedAtUtc { get; private set; }

    // Set when this token is rotated: points at the successor token that replaced it.
    public Guid? ReplacedByTokenId { get; private set; }

    // EF needs a parameterless constructor to materialize the entity.
    protected RefreshToken()
    {
        TokenHash = string.Empty;
    }

    public RefreshToken(Guid userId, string tokenHash, DateTime createdAtUtc, DateTime expiresAtUtc)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Token hash cannot be empty.", nameof(tokenHash));

        if (expiresAtUtc <= createdAtUtc)
            throw new ArgumentException("Expiry must be after creation.", nameof(expiresAtUtc));

        UserId = userId;
        TokenHash = tokenHash;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public bool IsRevoked => RevokedAtUtc is not null;

    // True once this token has been rotated into a successor — the marker that makes
    // a later replay of THIS token a theft signal.
    public bool WasRotated => ReplacedByTokenId is not null;

    // Usable right now: not revoked and not past expiry. Time is passed in (not read
    // from the clock) so the entity stays pure and testable.
    public bool IsActive(DateTime utcNow) => !IsRevoked && utcNow < ExpiresAtUtc;

    public void Revoke(DateTime utcNow, Guid? replacedByTokenId = null)
    {
        if (IsRevoked)
            return; // idempotent — revoking an already-revoked token is a no-op

        RevokedAtUtc = utcNow;
        ReplacedByTokenId = replacedByTokenId;
    }
}
