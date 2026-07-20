using GameHub.Application.Users;
using GameHub.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres)
        {
            // The app-level pre-check missed a concurrent insert (TOCTOU race).
            // Translate the raw database violation into a domain-meaningful
            // exception so the EF/Npgsql types stay sealed inside Infrastructure.
            if (postgres.ConstraintName == UserConstraintNames.UniqueEmail)
                throw new DuplicateEmailException();

            if (postgres.ConstraintName == UserConstraintNames.UniqueUsername)
                throw new DuplicateUsernameException();

            throw; // a unique violation we don't specifically handle — preserve the original
        }
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        _dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default) =>
        _dbContext.Users.AnyAsync(u => u.Username == username, cancellationToken);
}
