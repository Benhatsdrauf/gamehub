using GameHub.API.Contracts.Users;
using GameHub.Application.Users.RegisterUser;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.API.Controllers;

public sealed class UsersController : ApiController
{
    private readonly RegisterUserHandler _registerUserHandler;

    public UsersController(RegisterUserHandler registerUserHandler)
    {
        _registerUserHandler = registerUserHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Register(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            request.Username,
            request.Email,
            request.Password);

        var result = await _registerUserHandler.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Created($"/api/users/{result.Value.Id}", result.Value)
            : Problem(result.Error);
    }
}
