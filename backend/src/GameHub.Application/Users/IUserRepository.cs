using GameHub.Domain.Users;

namespace GameHub.Application.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);

    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    Task DeleteAsync(User user, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsForOtherUserAsync(string email, Guid userId, CancellationToken cancellationToken = default);

    Task<bool> UsernameExistsForOtherUserAsync(string username, Guid userId, CancellationToken cancellationToken = default);
}
