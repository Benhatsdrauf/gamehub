using GameHub.Application.Users;
using GameHub.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly GameHubDbContext _dbContext;

    public UserRepository(GameHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        _dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
}
