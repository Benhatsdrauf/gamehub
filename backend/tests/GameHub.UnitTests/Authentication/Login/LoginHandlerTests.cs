using GameHub.Application.Abstractions.Security;
using GameHub.Application.Authentication;
using GameHub.Application.Authentication.Login;
using GameHub.Application.Common.Errors;
using GameHub.Application.Users;
using GameHub.Domain.Users;
using NSubstitute;

namespace GameHub.UnitTests.Authentication.Login;

// LoginHandler coordinates THREE dependencies — repo, hasher, token generator. Each
// test stubs only what that path needs and asserts on both the Result and the calls.
public class LoginHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly LoginHandler _sut;

    public LoginHandlerTests()
    {
        _sut = new LoginHandler(_users, _passwordHasher, _tokenGenerator);
    }

    [Fact]
    public async Task Handle_UnknownEmail_ReturnsUnauthorized_StillRunsVerify_MintsNoToken()
    {
        // Arrange — GetByEmailAsync returns null by default (no such user).
        var command = new LoginCommand("nobody@example.com", "whatever123");

        // Act
        var result = await _sut.Handle(command);

        // Assert — vague 401
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);

        // Timing defense: a bcrypt Verify STILL ran (against the dummy hash), so this
        // path costs the same as a real one. And no token was minted.
        _passwordHasher.Received(1).Verify("whatever123", Arg.Any<string>());
        _tokenGenerator.DidNotReceive().GenerateAccessToken(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsInvalidCredentials_MintsNoToken()
    {
        // Arrange — user exists, but the password does not verify.
        var user = new User("alice", "alice@example.com", "STORED_HASH");
        _users.GetByEmailAsync("alice@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("wrongpass", "STORED_HASH").Returns(false);

        // Act
        var result = await _sut.Handle(new LoginCommand("alice@example.com", "wrongpass"));

        // Assert — same vague error as the unknown-email case
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);
        _tokenGenerator.DidNotReceive().GenerateAccessToken(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokenAndUserDetails()
    {
        // Arrange — user exists, password verifies, generator returns a known token.
        var user = new User("alice", "alice@example.com", "STORED_HASH");
        var expiry = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _users.GetByEmailAsync("alice@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("correct123", "STORED_HASH").Returns(true);
        _tokenGenerator.GenerateAccessToken(user).Returns(new AccessToken("JWT_TOKEN", expiry));

        // Act
        var result = await _sut.Handle(new LoginCommand("alice@example.com", "correct123"));

        // Assert — the response carries the token + basic identity
        Assert.True(result.IsSuccess);
        Assert.Equal("JWT_TOKEN", result.Value.AccessToken);
        Assert.Equal(expiry, result.Value.ExpiresAtUtc);
        Assert.Equal("alice@example.com", result.Value.Email);
        Assert.Equal("User", result.Value.Role);
    }
}
