using GameHub.Application.Common.Messaging;
using GameHub.Application.Common.Results;

namespace GameHub.Application.Users.GetUser;

public sealed class GetUserHandler
    : IQueryHandler<GetUserQuery, Result<GetUserResponse>>
{
    private readonly IUserQueries _userQueries;

    public GetUserHandler(IUserQueries userQueries)
    {
        _userQueries = userQueries;
    }

    public async Task<Result<GetUserResponse>> Handle(
        GetUserQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await _userQueries.GetByIdAsync(query.Id, cancellationToken);

        return user is null
            ? Result.Failure<GetUserResponse>(UserErrors.NotFound(query.Id))
            : Result.Success(user);
    }
}
