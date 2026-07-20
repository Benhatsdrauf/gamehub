using GameHub.Application.Abstractions.Security;
using GameHub.Application.Common.Results;
using GameHub.Domain.Users;

namespace GameHub.Application.Users.RegisterUser;

public sealed class RegisterUserHandler
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserHandler(IUserRepository users, IPasswordHasher passwordHasher)
    {
        _users = users;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<RegisterUserResponse>> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        if (await _users.EmailExistsAsync(command.Email, cancellationToken))
            return Result.Failure<RegisterUserResponse>(UserErrors.EmailNotUnique(command.Email));

        var passwordHash = _passwordHasher.Hash(command.Password);

        var user = new User(command.Username, command.Email, passwordHash);

        await _users.AddAsync(user, cancellationToken);

        var response = new RegisterUserResponse(user.Id, user.Username, user.Email);

        return Result.Success(response);
    }
}
