using GameHub.Application.Common.Messaging;
using GameHub.Application.Common.Results;

namespace GameHub.Application.Authentication.Logout;

public sealed record LogoutCommand(string RefreshToken) : ICommand<Result>;
