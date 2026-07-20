using GameHub.API.Contracts.Users;
using GameHub.Application.Users.GetUser;
using GameHub.Application.Users.RegisterUser;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.API.Controllers;

public sealed class UsersController : ApiController
{
    private readonly RegisterUserHandler _registerUserHandler;
    private readonly GetUserHandler _getUserHandler;

    public UsersController(
        RegisterUserHandler registerUserHandler,
        GetUserHandler getUserHandler)
    {
        _registerUserHandler = registerUserHandler;
        _getUserHandler = getUserHandler;
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
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : Problem(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getUserHandler.Handle(new GetUserQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error);
    }
}
