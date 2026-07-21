using GameHub.Domain.Users;

namespace GameHub.Application.Abstractions.Security;

// A port (in the Ports & Adapters sense): the Application says WHAT it needs — an
// access token for a given user — without knowing HOW. The "how" (which library,
// which algorithm, where the secret lives) is an Infrastructure detail. Twin of
// IPasswordHasher.
public interface IJwtTokenGenerator
{
    AccessToken GenerateAccessToken(User user);
}
