namespace GameHub.Application.Abstractions.Security;

// The result of generating an access token: the signed JWT string plus the moment
// it expires. Returning both keeps a single source of truth for the expiry — the
// generator owns it (from config), and the caller just reports it to the client.
public sealed record AccessToken(string Value, DateTime ExpiresAtUtc);
