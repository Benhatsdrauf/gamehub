using GameHub.API.Contracts.Auth;
using GameHub.Application.Authentication.Login;
using GameHub.Application.Authentication.Logout;
using GameHub.Application.Authentication.Refresh;
using GameHub.Application.Common.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.API.Controllers;

public sealed class AuthController : ApiController
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);

        var result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        RefreshRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RefreshTokenCommand(request.RefreshToken), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        LogoutRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new LogoutCommand(request.RefreshToken), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : Problem(result.Error);
    }
}
