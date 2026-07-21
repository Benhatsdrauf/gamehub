using GameHub.API.Contracts.Users;
using GameHub.Application.Common.Messaging;
using GameHub.Application.Users.DeleteUser;
using GameHub.Application.Users.GetUser;
using GameHub.Application.Users.GetUsers;
using GameHub.Application.Users.RegisterUser;
using GameHub.Application.Users.UpdateUser;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.API.Controllers;

public sealed class UsersController : ApiController
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
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

        var result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : Problem(result.Error);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetUsersQuery(page, pageSize), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUserQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand(id, request.Username, request.Email);

        var result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteUserCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : Problem(result.Error);
    }
}
