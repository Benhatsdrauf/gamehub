using GameHub.API.Contracts.Auth;
using GameHub.Application.Authentication.Login;
using GameHub.Application.Common.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.API.Controllers;

public sealed class AuthController : ApiController
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

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
}
