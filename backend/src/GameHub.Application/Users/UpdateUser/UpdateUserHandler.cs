using GameHub.Application.Common.Messaging;
using GameHub.Application.Common.Results;

namespace GameHub.Application.Users.UpdateUser;

public sealed class UpdateUserHandler
    : ICommandHandler<UpdateUserCommand, Result<UpdateUserResponse>>
{
    private readonly IUserRepository _users;

    public UpdateUserHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<Result<UpdateUserResponse>> Handle(
        UpdateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validation now runs in ValidationBehavior before this handler is reached.
        // Load the tracked entity — the write side, not IUserQueries (which returns a DTO).
        var user = await _users.GetByIdAsync(command.Id, cancellationToken);
        if (user is null)
            return Result.Failure<UpdateUserResponse>(UserErrors.NotFound(command.Id));

        // Uniqueness checks must EXCLUDE this user, or we'd flag their own row as a conflict.
        if (await _users.EmailExistsForOtherUserAsync(command.Email, command.Id, cancellationToken))
            return Result.Failure<UpdateUserResponse>(UserErrors.EmailNotUnique(command.Email));

        if (await _users.UsernameExistsForOtherUserAsync(command.Username, command.Id, cancellationToken))
            return Result.Failure<UpdateUserResponse>(UserErrors.UsernameNotUnique(command.Username));

        // Mutate through domain methods — the entity enforces its own invariants.
        user.Rename(command.Username);
        user.UpdateEmail(command.Email);

        try
        {
            await _users.UpdateAsync(user, cancellationToken);
        }
        catch (DuplicateEmailException)
        {
            return Result.Failure<UpdateUserResponse>(UserErrors.EmailNotUnique(command.Email));
        }
        catch (DuplicateUsernameException)
        {
            return Result.Failure<UpdateUserResponse>(UserErrors.UsernameNotUnique(command.Username));
        }

        var response = new UpdateUserResponse(user.Id, user.Username, user.Email);

        return Result.Success(response);
    }
}
