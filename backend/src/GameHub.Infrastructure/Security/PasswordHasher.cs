using GameHub.Application.Abstractions.Security;

namespace GameHub.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    // Work factor is a server-owned security parameter: higher = slower to
    // compute = more resistant to brute force. Raise it over time as hardware
    // gets faster. Changing it does not break existing hashes — bcrypt stores
    // the factor inside each hash, so Verify still works against older values.
    private const int WorkFactor = 12;

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string passwordHash) =>
        BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
