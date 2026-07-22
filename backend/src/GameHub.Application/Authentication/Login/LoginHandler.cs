using GameHub.Application.Abstractions.Security;
using GameHub.Application.Common.Messaging;
using GameHub.Application.Common.Results;
using GameHub.Application.Users;

namespace GameHub.Application.Authentication.Login;

public sealed class LoginHandler
    : ICommandHandler<LoginCommand, Result<LoginResponse>>
{
    // A fixed, valid bcrypt hash (work factor 12) of a throwaway value — NOT any real
    // user's password. Verified against on the no-such-user path so that path costs
    // roughly the same bcrypt time as a real verification (see the timing note below).
    private const string DummyPasswordHash =
        "$2a$12$V9MbDyxBCY45P.Nqch8IH.UJGYp5P0aF31vpvn9VVZaSAd/TJlOcW";

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public LoginHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validation now runs in ValidationBehavior before this handler is reached.
        //
        // Both failure branches below return the SAME error on purpose — never leak
        // whether it was the email or the password that didn't match.
        var user = await _users.GetByEmailAsync(command.Email, cancellationToken);
        if (user is null)
        {
            // Timing defense: there is no user, but still run one bcrypt Verify against
            // a fixed dummy hash so this path costs about the same as the found-user
            // path. Without it, "email exists" (slow bcrypt) is distinguishable from
            // "email doesn't" (instant) by measuring response time — a side-channel that
            // partly defeats the vague error above. The result is intentionally ignored.
            _passwordHasher.Verify(command.Password, DummyPasswordHash);
            return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);
        }

        if (!_passwordHasher.Verify(command.Password, user.PasswordHash))
            return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);

        var token = _tokenGenerator.GenerateAccessToken(user);

        var response = new LoginResponse(
            token.Value,
            token.ExpiresAtUtc,
            user.Id,
            user.Username,
            user.Email,
            user.Role.ToString());

        return Result.Success(response);
    }
}
