namespace GameHub.Infrastructure.Security;

// A typed view of the "Jwt" configuration section. We bind the section to this
// object once (in DependencyInjection) and inject it via IOptions<JwtSettings>,
// instead of reading stringly-typed keys out of IConfiguration all over the code.
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    // The signing secret. NEVER committed — supplied by user-secrets in dev and by
    // an environment variable in production. Everything else here is safe in appsettings.
    public string Secret { get; init; } = string.Empty;

    // Who issued the token (this API) and who it's meant for. Validated on the way
    // back in, so a token minted for another system is rejected.
    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    // How long an access token stays valid. Short, because the token is a snapshot
    // of the user (see role staleness) — a refresh flow will re-mint it later.
    public int AccessTokenMinutes { get; init; } = 60;
}
