using GameHub.Application.Common.Messaging;
using GameHub.Application.Common.Pagination;
using GameHub.Application.Common.Results;

namespace GameHub.Application.Users.GetUsers;

public sealed class GetUsersHandler
    : IQueryHandler<GetUsersQuery, Result<PagedResponse<UserListItem>>>
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly IUserQueries _userQueries;

    public GetUsersHandler(IUserQueries userQueries)
    {
        _userQueries = userQueries;
    }

    public async Task<Result<PagedResponse<UserListItem>>> Handle(
        GetUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        // Guardrails: never trust the client's paging values. Clamp page to >= 1
        // and pageSize into [1, MaxPageSize] so nobody can request a million rows.
        var page = query.Page < 1 ? 1 : query.Page;

        var pageSize = query.PageSize switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => query.PageSize
        };

        var users = await _userQueries.GetUsersAsync(page, pageSize, cancellationToken);

        return Result.Success(users);
    }
}
