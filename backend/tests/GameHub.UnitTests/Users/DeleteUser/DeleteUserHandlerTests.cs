using GameHub.Application.Common.Errors;
using GameHub.Application.Users;
using GameHub.Application.Users.DeleteUser;
using GameHub.Domain.Users;
using NSubstitute;

namespace GameHub.UnitTests.Users.DeleteUser;

public class DeleteUserHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly DeleteUserHandler _sut;

    public DeleteUserHandlerTests()
    {
        _sut = new DeleteUserHandler(_users);
    }

    [Fact]
    public async Task Handle_UserMissing_ReturnsNotFound_AndDeletesNothing()
    {
        // Arrange — GetByIdAsync returns null by default
        // Act
        var result = await _sut.Handle(new DeleteUserCommand(Guid.NewGuid()));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        await _users.DidNotReceive().DeleteAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserExists_DeletesIt()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = new User("alice", "alice@example.com", "HASH");
        _users.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.Handle(new DeleteUserCommand(id));

        // Assert
        Assert.True(result.IsSuccess);
        await _users.Received(1).DeleteAsync(user, Arg.Any<CancellationToken>());
    }
}
