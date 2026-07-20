using GameHub.Application.Common.Pagination;
using GameHub.Application.Users.GetUser;
using GameHub.Application.Users.GetUsers;

namespace GameHub.Application.Users;

// The read side. Unlike IUserRepository (which deals in the User entity), every
// method here returns a projected DTO — never the domain entity.
public interface IUserQueries
{
    Task<GetUserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResponse<UserListItem>> GetUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
