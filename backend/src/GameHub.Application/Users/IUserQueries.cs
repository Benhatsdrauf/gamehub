using GameHub.Application.Users.GetUser;

namespace GameHub.Application.Users;

// The read side. Unlike IUserRepository (which deals in the User entity), every
// method here returns a projected DTO — never the domain entity.
public interface IUserQueries
{
    Task<GetUserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
