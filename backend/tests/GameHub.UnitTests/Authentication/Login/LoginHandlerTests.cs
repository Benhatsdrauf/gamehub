using GameHub.Application.Abstractions.Security;
using GameHub.Application.Authentication;
using GameHub.Application.Authentication.Login;
using GameHub.Application.Common.Errors;
using GameHub.Application.Users;
using GameHub.Domain.Authentication;
using GameHub.Domain.Users;
using NSubstitute;

namespace GameHub.UnitTests.Authentication.Login;

// LoginHandler coordinates FIVE dependencies now — repo, hasher, access-token generator,
// refresh-token generator, refresh repo. Each test stubs only what its path needs.
public class LoginHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IRefreshTokenGenerator _refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly LoginHandler _sut;

    public LoginHandlerTests()
    {
        _sut = new LoginHandler(_users, _passwordHasher, _tokenGenerator, _refreshTokenGenerator, _refreshTokens);
    }

    [Fact]
    public async Task Handle_UnknownEmail_ReturnsUnauthorized_StillRunsVerify_IssuesNothing()
    {
        // Arrange — GetByEmailAsync returns null by default (no such user).
        var command = new LoginCommand("nobody@example.com", "whatever123");

        // Act
        var result = await _sut.Handle(command);

        // Assert — vague 401
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);

        // Timing defense still runs a Verify; but no tokens are issued or stored.
        _passwordHasher.Received(1).Verify("whatever123", Arg.Any<string>());
        _tokenGenerator.DidNotReceive().GenerateAccessToken(Arg.Any<User>());
        _refreshTokenGenerator.DidNotReceive().Create(Arg.Any<Guid>());
        _refreshTokens.DidNotReceive().Add(Arg.Any<RefreshToken>());
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsInvalidCredentials_IssuesNothing()
    {
        // Arrange — user exists, but the password does not verify.
        var user = new User("alice", "alice@example.com", "STORED_HASH");
        _users.GetByEmailAsync("alice@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("wrongpass", "STORED_HASH").Returns(false);

        // Act
        var result = await _sut.Handle(new LoginCommand("alice@example.com", "wrongpass"));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);
        _tokenGenerator.DidNotReceive().GenerateAccessToken(Arg.Any<User>());
        _refreshTokens.DidNotReceive().Add(Arg.Any<RefreshToken>());
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsBothTokens_AndStoresTheRefreshToken()
    {
        // Arrange — user exists, password verifies, both generators return known tokens.
        var user = new User("alice", "alice@example.com", "STORED_HASH");
        var accessExpiry = new DateTime(2030, 1, 1, 1, 0, 0, DateTimeKind.Utc);
        var created = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var refreshExpiry = created.AddDays(7);
        var refreshEntity = new RefreshToken(user.Id, "REFRESH_HASH", created, refreshExpiry);

        _users.GetByEmailAsync("alice@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("correct123", "STORED_HASH").Returns(true);
        _tokenGenerator.GenerateAccessToken(user).Returns(new AccessToken("JWT_TOKEN", accessExpiry));
        _refreshTokenGenerator.Create(user.Id).Returns(new NewRefreshToken(refreshEntity, "RAW_REFRESH"));

        // Act
        var result = await _sut.Handle(new LoginCommand("alice@example.com", "correct123"));

        // Assert — the response carries both tokens + basic identity
        Assert.True(result.IsSuccess);
        Assert.Equal("JWT_TOKEN", result.Value.AccessToken);
        Assert.Equal(accessExpiry, result.Value.AccessTokenExpiresAtUtc);
        Assert.Equal("RAW_REFRESH", result.Value.RefreshToken);
        Assert.Equal(refreshExpiry, result.Value.RefreshTokenExpiresAtUtc);
        Assert.Equal("User", result.Value.Role);

        // The refresh token was persisted.
        _refreshTokens.Received(1).Add(refreshEntity);
        await _refreshTokens.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
