using GameHub.Application.Common.Messaging;
using GameHub.Application.Common.Results;

namespace GameHub.Application.Users.PromoteUser;

public sealed record PromoteUserCommand(Guid Id) : ICommand<Result>;
