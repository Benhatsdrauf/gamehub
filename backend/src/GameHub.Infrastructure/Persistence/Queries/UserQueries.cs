using GameHub.Application.Users;
using GameHub.Application.Users.GetUser;
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
}
