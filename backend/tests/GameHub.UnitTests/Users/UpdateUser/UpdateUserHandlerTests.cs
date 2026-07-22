using GameHub.Application.Common.Errors;
using GameHub.Application.Users;
using GameHub.Application.Users.UpdateUser;
using GameHub.Domain.Users;
using NSubstitute;

namespace GameHub.UnitTests.Users.UpdateUser;

public class UpdateUserHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly UpdateUserHandler _sut;

    public UpdateUserHandlerTests()
    {
        _sut = new UpdateUserHandler(_users);
    }

    [Fact]
    public async Task Handle_UserMissing_ReturnsNotFound()
    {
        // Arrange — GetByIdAsync returns null by default
        var command = new UpdateUserCommand(Guid.NewGuid(), "newname", "new@example.com");

        // Act
        var result = await _sut.Handle(command);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_EmailTakenByAnotherUser_ReturnsConflict_AndDoesNotSave()
    {
        // Arrange — the user exists, but the new email belongs to someone else.
        var id = Guid.NewGuid();
        var user = new User("old", "old@example.com", "HASH");
        _users.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(user);
        _users.EmailExistsForOtherUserAsync("taken@example.com", id, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.Handle(new UpdateUserCommand(id, "old", "taken@example.com"));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.EmailNotUnique", result.Error.Code);
        await _users.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidChange_AppliesItAndSaves()
    {
        // Arrange — user exists, no conflicts (existence checks default to false).
        var id = Guid.NewGuid();
        var user = new User("old", "old@example.com", "HASH");
        _users.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.Handle(new UpdateUserCommand(id, "newname", "new@example.com"));

        // Assert — the response reflects the applied change, and it was persisted.
        Assert.True(result.IsSuccess);
        Assert.Equal("newname", result.Value.Username);
        Assert.Equal("new@example.com", result.Value.Email);
        await _users.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }
}
