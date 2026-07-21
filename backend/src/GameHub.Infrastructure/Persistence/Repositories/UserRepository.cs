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

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        // No projection, no AsNoTracking: EF tracks this entity, so later mutations
        // are detected and persisted on SaveChanges.
        _dbContext.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        // AsNoTracking: login only reads the user to verify the password; it never
        // mutates it, so EF can skip building a change-tracking snapshot.
        _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Add(user);
        await SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default) =>
        // The user was loaded via GetByIdAsync in this same request, so it is already
        // tracked. We do NOT call Update() (that force-marks every column modified);
        // SaveChanges writes only the columns that actually changed.
        SaveChangesAsync(cancellationToken);

    public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        // Remove marks the tracked entity as Deleted; SaveChanges issues the DELETE.
        _dbContext.Users.Remove(user);
        await SaveChangesAsync(cancellationToken);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        _dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken);

    public Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default) =>
        _dbContext.Users.AnyAsync(user => user.Username == username, cancellationToken);

    public Task<bool> EmailExistsForOtherUserAsync(string email, Guid userId, CancellationToken cancellationToken = default) =>
        _dbContext.Users.AnyAsync(user => user.Email == email && user.Id != userId, cancellationToken);

    public Task<bool> UsernameExistsForOtherUserAsync(string username, Guid userId, CancellationToken cancellationToken = default) =>
        _dbContext.Users.AnyAsync(user => user.Username == username && user.Id != userId, cancellationToken);

    // Shared by AddAsync and UpdateAsync: persists pending changes and translates a
    // database unique-violation (the race-loser) into a domain exception, so EF/Npgsql
    // types never escape Infrastructure.
    private async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres)
        {
            if (postgres.ConstraintName == UserConstraintNames.UniqueEmail)
                throw new DuplicateEmailException();

            if (postgres.ConstraintName == UserConstraintNames.UniqueUsername)
                throw new DuplicateUsernameException();

            throw;
        }
    }
}
