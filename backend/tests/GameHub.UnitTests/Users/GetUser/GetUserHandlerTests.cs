using GameHub.Application.Common.Errors;
using GameHub.Application.Users;
using GameHub.Application.Users.GetUser;
using NSubstitute;

namespace GameHub.UnitTests.Users.GetUser;

public class GetUserHandlerTests
{
    private readonly IUserQueries _queries = Substitute.For<IUserQueries>();
    private readonly GetUserHandler _sut;

    public GetUserHandlerTests()
    {
        _sut = new GetUserHandler(_queries);
    }

    [Fact]
    public async Task Handle_UserExists_ReturnsIt()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new GetUserResponse(id, "alice", "alice@example.com");
        _queries.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(dto);

        // Act
        var result = await _sut.Handle(new GetUserQuery(id));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(dto, result.Value);
    }

    [Fact]
    public async Task Handle_UserMissing_ReturnsNotFound()
    {
        // Arrange — GetByIdAsync returns null by default
        // Act
        var result = await _sut.Handle(new GetUserQuery(Guid.NewGuid()));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
