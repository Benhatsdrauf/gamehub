using System.Security.Cryptography;
using System.Text;
using GameHub.Application.Abstractions.Security;
using GameHub.Domain.Authentication;
using Microsoft.Extensions.Options;

namespace GameHub.Infrastructure.Security;

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    // 256 bits of entropy — infeasible to guess or brute-force, which is why the fast
    // SHA-256 hash below is safe for storage.
    private const int TokenSizeBytes = 32;

    private readonly JwtSettings _settings;

    public RefreshTokenGenerator(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public NewRefreshToken Create(Guid userId)
    {
        // RandomNumberGenerator is a cryptographically secure RNG (unlike Random).
        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(TokenSizeBytes));

        var now = DateTime.UtcNow;
        var expiresAt = now.AddDays(_settings.RefreshTokenDays);

        // Store only the hash; the raw token leaves in NewRefreshToken and is never persisted.
        var token = new RefreshToken(userId, Hash(rawToken), now, expiresAt);

        return new NewRefreshToken(token, rawToken);
    }

    // Deterministic + fast. Fast is fine here because the token is high-entropy random —
    // there is nothing to brute-force (bcrypt's slowness only helps LOW-entropy secrets).
    public string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
