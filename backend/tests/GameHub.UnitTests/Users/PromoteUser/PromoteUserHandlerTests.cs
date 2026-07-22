using GameHub.Application.Common.Errors;
using GameHub.Application.Users;
using GameHub.Application.Users.PromoteUser;
using GameHub.Domain.Enums;
using GameHub.Domain.Users;
using NSubstitute;

namespace GameHub.UnitTests.Users.PromoteUser;

public class PromoteUserHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly PromoteUserHandler _sut;

    public PromoteUserHandlerTests()
    {
        _sut = new PromoteUserHandler(_users);
    }

    [Fact]
    public async Task Handle_UserMissing_ReturnsNotFound()
    {
        // Arrange — GetByIdAsync returns null by default
        // Act
        var result = await _sut.Handle(new PromoteUserCommand(Guid.NewGuid()));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_UserExists_PromotesToAdminAndSaves()
    {
        // Arrange — a normal user (Role defaults to User).
        var id = Guid.NewGuid();
        var user = new User("alice", "alice@example.com", "HASH");
        _users.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.Handle(new PromoteUserCommand(id));

        // Assert — the domain state actually changed, and it was persisted.
        Assert.True(result.IsSuccess);
        Assert.Equal(UserRole.Admin, user.Role);
        await _users.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }
}
