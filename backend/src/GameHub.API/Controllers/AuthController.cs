using GameHub.API.Contracts.Auth;
using GameHub.Application.Authentication.Login;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.API.Controllers;

public sealed class AuthController : ApiController
{
    private readonly LoginHandler _loginHandler;

    public AuthController(LoginHandler loginHandler)
    {
        _loginHandler = loginHandler;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);

        var result = await _loginHandler.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error);
    }
}
