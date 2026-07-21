using GameHub.Application.Common.Messaging;
using GameHub.Application.Common.Results;

namespace GameHub.Application.Users.GetUser;

public sealed record GetUserQuery(Guid Id) : IQuery<Result<GetUserResponse>>;
