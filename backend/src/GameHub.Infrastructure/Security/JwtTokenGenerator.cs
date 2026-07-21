using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GameHub.Application.Abstractions.Security;
using GameHub.Domain.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GameHub.Infrastructure.Security;

// The adapter behind IJwtTokenGenerator. This is the ONLY place that knows about
// the JWT library, the signing algorithm, and the secret. Application stays clean.
public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _settings;

    // IOptions<T> is how the bound JwtSettings is delivered. .Value is the object.
    public JwtTokenGenerator(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public AccessToken GenerateAccessToken(User user)
    {
        // Compute "now" once so nbf/exp are consistent within the token.
        var now = DateTime.UtcNow;
        var expiresAtUtc = now.AddMinutes(_settings.AccessTokenMinutes);

        // The PAYLOAD: the claims. Remember these are readable by anyone holding the
        // token — nothing secret goes here.
        //   sub  = the subject: the stable user id (the real identity)
        //   email/role = convenience + coarse RBAC (a login-time snapshot)
        //   jti  = a unique id per token; handy later for revocation/refresh
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("role", user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // The SIGNATURE ingredients: turn the shared secret into a key and pair it
        // with HMAC-SHA256. The same secret is used later to verify the signature.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Assemble header + payload + signing instructions. issuer/audience/expiry
        // become the standard iss/aud/exp claims; the handler adds iat automatically.
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        // WriteToken does the Base64URL-encode-and-sign, producing the final
        // "header.payload.signature" string the client will send back.
        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return new AccessToken(tokenValue, expiresAtUtc);
    }
}
