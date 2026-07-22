using GameHub.Application.Common.Messaging;
using GameHub.Application.Common.Results;

namespace GameHub.Application.Users.PromoteUser;

public sealed class PromoteUserHandler
    : ICommandHandler<PromoteUserCommand, Result>
{
    private readonly IUserRepository _users;

    public PromoteUserHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<Result> Handle(
        PromoteUserCommand command,
        CancellationToken cancellationToken = default)
    {
        // Tracked entity (not AsNoTracking) — we're about to mutate and persist it.
        var user = await _users.GetByIdAsync(command.Id, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.NotFound(command.Id));

        // Let the entity own the transition; the handler doesn't set Role directly.
        // Already-admin is harmless (idempotent) — PromoteToAdmin just sets Admin again.
        user.PromoteToAdmin();

        await _users.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
