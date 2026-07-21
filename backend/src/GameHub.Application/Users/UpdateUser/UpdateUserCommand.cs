using GameHub.Application.Common.Messaging;
using GameHub.Application.Common.Results;

namespace GameHub.Application.Users.UpdateUser;

public sealed record UpdateUserCommand(
    Guid Id,
    string Username,
    string Email) : ICommand<Result<UpdateUserResponse>>;
