using GameHub.Application.Common.Messaging;
using GameHub.Application.Common.Results;

namespace GameHub.Application.Users.DeleteUser;

public sealed record DeleteUserCommand(Guid Id) : ICommand<Result>;
