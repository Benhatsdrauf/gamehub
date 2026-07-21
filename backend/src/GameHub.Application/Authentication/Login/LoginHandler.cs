using FluentValidation;
using GameHub.Application.Abstractions.Security;
using GameHub.Application.Common.Errors;
using GameHub.Application.Common.Results;
using GameHub.Application.Users;

namespace GameHub.Application.Authentication.Login;

public sealed class LoginHandler
{
    private readonly IValidator<LoginCommand> _validator;
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public LoginHandler(
        IValidator<LoginCommand> validator,
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator)
    {
        _validator = validator;
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        // TODO(mediator): this ~10-line validation block is now copy-pasted across
        // RegisterUser, UpdateUser, and Login (3rd copy). Once Login lands, extract it
        // into a validation pipeline behavior in our own in-house mediator so it lives
        // in exactly one place instead of every handler.
        var validation = await _validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).ToArray());

            return Result.Failure<LoginResponse>(new ValidationError(errors));
        }

        // Both failure branches below return the SAME error on purpose — never leak
        // whether it was the email or the password that didn't match.
        //
        // TODO(timing): the vague error still has a timing side-channel. The null-user
        // path returns immediately, while the found-user path runs bcrypt Verify (slow
        // by design). An attacker timing responses can still tell "email exists" from
        // "email doesn't". Fix later by running a throwaway Verify against a fixed dummy
        // hash on the null-user path so both branches take roughly the same time.
        var user = await _users.GetByEmailAsync(command.Email, cancellationToken);
        if (user is null)
            return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);

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
