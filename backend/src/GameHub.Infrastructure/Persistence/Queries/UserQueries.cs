using GameHub.Application.Common.Pagination;
using GameHub.Application.Users;
using GameHub.Application.Users.GetUser;
using GameHub.Application.Users.GetUsers;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Infrastructure.Persistence.Queries;

public sealed class UserQueries : IUserQueries
{
    private readonly GameHubDbContext _dbContext;

    public UserQueries(GameHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<GetUserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Users
            .Where(user => user.Id == id)
            // Projecting into a non-entity type: EF selects only these columns
            // (PasswordHash never leaves the DB) and returns untracked results.
            .Select(user => new GetUserResponse(user.Id, user.Username, user.Email))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<PagedResponse<UserListItem>> GetUsersAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await _dbContext.Users.CountAsync(cancellationToken);

        var items = await _dbContext.Users
            .OrderBy(user => user.Username)          // ORDER BY is required for stable paging
            .Skip((page - 1) * pageSize)             // OFFSET
            .Take(pageSize)                          // LIMIT
            .Select(user => new UserListItem(user.Id, user.Username, user.Email))
            .ToListAsync(cancellationToken);

        return new PagedResponse<UserListItem>(items, page, pageSize, totalCount);
    }
}
