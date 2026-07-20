using GameHub.Application.Common.Results;

namespace GameHub.Application.Users.DeleteUser;

public sealed class DeleteUserHandler
{
    private readonly IUserRepository _users;

    public DeleteUserHandler(IUserRepository users)
    {
        _users = users;
    }

    // Non-generic Result: a successful delete has no value to return.
    public async Task<Result> Handle(
        DeleteUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(command.Id, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.NotFound(command.Id));

        await _users.DeleteAsync(user, cancellationToken);

        return Result.Success();
    }
}
