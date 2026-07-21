using GameHub.Application.Abstractions.Security;
using GameHub.Application.Common.Messaging;
using GameHub.Application.Common.Results;
using GameHub.Domain.Users;

namespace GameHub.Application.Users.RegisterUser;

public sealed class RegisterUserHandler
    : ICommandHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher)
    {
        _users = users;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<RegisterUserResponse>> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validation now runs in ValidationBehavior before this handler is reached.
        if (await _users.EmailExistsAsync(command.Email, cancellationToken))
            return Result.Failure<RegisterUserResponse>(UserErrors.EmailNotUnique(command.Email));

        if (await _users.UsernameExistsAsync(command.Username, cancellationToken))
            return Result.Failure<RegisterUserResponse>(UserErrors.UsernameNotUnique(command.Username));

        var passwordHash = _passwordHasher.Hash(command.Password);

        var user = new User(command.Username, command.Email, passwordHash);

        try
        {
            await _users.AddAsync(user, cancellationToken);
        }
        catch (DuplicateEmailException)
        {
            return Result.Failure<RegisterUserResponse>(UserErrors.EmailNotUnique(command.Email));
        }
        catch (DuplicateUsernameException)
        {
            return Result.Failure<RegisterUserResponse>(UserErrors.UsernameNotUnique(command.Username));
        }

        var response = new RegisterUserResponse(user.Id, user.Username, user.Email);

        return Result.Success(response);
    }
}
