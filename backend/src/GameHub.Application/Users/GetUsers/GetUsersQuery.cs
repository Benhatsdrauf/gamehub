using GameHub.Application.Common.Messaging;
using GameHub.Application.Common.Pagination;
using GameHub.Application.Common.Results;

namespace GameHub.Application.Users.GetUsers;

public sealed record GetUsersQuery(int Page, int PageSize)
    : IQuery<Result<PagedResponse<UserListItem>>>;
