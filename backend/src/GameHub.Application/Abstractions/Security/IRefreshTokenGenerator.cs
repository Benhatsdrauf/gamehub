namespace GameHub.Application.Abstractions.Security;

// Mints and hashes refresh tokens. Create() builds a whole token (entity + raw string)
// with its expiry baked in — so the Application never needs to know the refresh
// lifetime, exactly as IJwtTokenGenerator hides the access-token lifetime. Hash() lets
// the refresh flow look an incoming raw token up by its stored hash.
public interface IRefreshTokenGenerator
{
    NewRefreshToken Create(Guid userId);

    string Hash(string token);
}
