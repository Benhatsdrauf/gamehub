using GameHub.Application.Abstractions.Security;
using GameHub.Application.Authentication;
using GameHub.Application.Authentication.Refresh;
using GameHub.Application.Users;
using GameHub.Domain.Authentication;
using GameHub.Domain.Users;
using NSubstitute;

namespace GameHub.UnitTests.Authentication.Refresh;

public class RefreshTokenHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly IRefreshTokenGenerator _refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
    private readonly IJwtTokenGenerator _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly RefreshTokenHandler _sut;

    public RefreshTokenHandlerTests()
    {
        _sut = new RefreshTokenHandler(_refreshTokens, _refreshTokenGenerator, _tokenGenerator, _users);
    }

    [Fact]
    public async Task Handle_UnknownToken_ReturnsInvalid()
    {
        // Arrange — hash lookup finds nothing (GetByHashAsync returns null by default).
        _refreshTokenGenerator.Hash("raw").Returns("HASH");

        // Act
        var result = await _sut.Handle(new RefreshTokenCommand("raw"));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidRefreshToken", result.Error.Code);
    }

    [Fact]
    public async Task Handle_ExpiredButNotRotated_ReturnsInvalid_WithoutRevokingEverything()
    {
        // Arrange — a token past expiry that was never rotated (not a theft signal).
        var expired = new RefreshToken(
            Guid.NewGuid(), "HASH", DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));
        _refreshTokenGenerator.Hash("raw").Returns("HASH");
        _refreshTokens.GetByHashAsync("HASH", Arg.Any<CancellationToken>()).Returns(expired);

        // Act
        var result = await _sut.Handle(new RefreshTokenCommand("raw"));

        // Assert — invalid, but no mass revocation (that's only for replay of a used token)
        Assert.True(result.IsFailure);
        await _refreshTokens.DidNotReceive()
            .GetNonRevokedForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReplayedRotatedToken_RevokesAllUserTokens_ReturnsInvalid()
    {
        // Arrange — a token that was already rotated away (WasRotated) is presented again.
        var userId = Guid.NewGuid();
        var replayed = new RefreshToken(
            userId, "HASH", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(6));
        replayed.Revoke(DateTime.UtcNow, Guid.NewGuid()); // rotated earlier → WasRotated == true

        var liveToken = new RefreshToken(
            userId, "OTHER", DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddDays(6));

        _refreshTokenGenerator.Hash("raw").Returns("HASH");
        _refreshTokens.GetByHashAsync("HASH", Arg.Any<CancellationToken>()).Returns(replayed);
        _refreshTokens.GetNonRevokedForUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<RefreshToken> { liveToken });

        // Act
        var result = await _sut.Handle(new RefreshTokenCommand("raw"));

        // Assert — invalid, AND every live token for the user got revoked
        Assert.True(result.IsFailure);
        Assert.NotNull(liveToken.RevokedAtUtc);
        await _refreshTokens.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ActiveToken_RotatesAndReturnsNewPair()
    {
        // Arrange — an active token, a real user, and stubbed new tokens.
        var user = new User("alice", "alice@example.com", "HASH");
        var active = new RefreshToken(
            user.Id, "HASH", DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddDays(6));
        var newEntity = new RefreshToken(
            user.Id, "NEWHASH", DateTime.UtcNow, DateTime.UtcNow.AddDays(7));

        _refreshTokenGenerator.Hash("raw").Returns("HASH");
        _refreshTokens.GetByHashAsync("HASH", Arg.Any<CancellationToken>()).Returns(active);
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _tokenGenerator.GenerateAccessToken(user)
            .Returns(new AccessToken("ACCESS", DateTime.UtcNow.AddHours(1)));
        _refreshTokenGenerator.Create(user.Id).Returns(new NewRefreshToken(newEntity, "NEWRAW"));

        // Act
        var result = await _sut.Handle(new RefreshTokenCommand("raw"));

        // Assert — new pair returned, old token revoked and linked to its successor
        Assert.True(result.IsSuccess);
        Assert.Equal("ACCESS", result.Value.AccessToken);
        Assert.Equal("NEWRAW", result.Value.RefreshToken);
        Assert.NotNull(active.RevokedAtUtc);
        Assert.Equal(newEntity.Id, active.ReplacedByTokenId);
        _refreshTokens.Received(1).Add(newEntity);
        await _refreshTokens.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
