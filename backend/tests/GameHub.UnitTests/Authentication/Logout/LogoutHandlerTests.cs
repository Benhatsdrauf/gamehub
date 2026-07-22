using GameHub.Application.Abstractions.Security;
using GameHub.Application.Authentication;
using GameHub.Application.Authentication.Logout;
using GameHub.Domain.Authentication;
using NSubstitute;

namespace GameHub.UnitTests.Authentication.Logout;

public class LogoutHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly IRefreshTokenGenerator _refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
    private readonly LogoutHandler _sut;

    public LogoutHandlerTests()
    {
        _sut = new LogoutHandler(_refreshTokens, _refreshTokenGenerator);
    }

    [Fact]
    public async Task Handle_LiveToken_RevokesItAndSaves()
    {
        // Arrange
        var token = new RefreshToken(
            Guid.NewGuid(), "HASH", DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddDays(6));
        _refreshTokenGenerator.Hash("raw").Returns("HASH");
        _refreshTokens.GetByHashAsync("HASH", Arg.Any<CancellationToken>()).Returns(token);

        // Act
        var result = await _sut.Handle(new LogoutCommand("raw"));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(token.RevokedAtUtc);
        await _refreshTokens.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownToken_ReturnsSuccess_WithoutSaving()
    {
        // Arrange — GetByHashAsync returns null by default. Logout is idempotent and
        // must not reveal whether the token existed.
        _refreshTokenGenerator.Hash("raw").Returns("HASH");

        // Act
        var result = await _sut.Handle(new LogoutCommand("raw"));

        // Assert
        Assert.True(result.IsSuccess);
        await _refreshTokens.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
