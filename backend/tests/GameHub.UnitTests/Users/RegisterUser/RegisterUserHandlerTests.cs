using GameHub.Application.Abstractions.Security;
using GameHub.Application.Common.Errors;
using GameHub.Application.Users;
using GameHub.Application.Users.RegisterUser;
using GameHub.Domain.Users;
using NSubstitute;

namespace GameHub.UnitTests.Users.RegisterUser;

// The handler depends only on interfaces (IUserRepository, IPasswordHasher) — the
// ports we built. That is exactly what lets us test it with fakes: no Postgres, no
// real bcrypt, just the handler's own logic.
public class RegisterUserHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly RegisterUserHandler _sut; // "system under test"

    public RegisterUserHandlerTests()
    {
        _sut = new RegisterUserHandler(_users, _passwordHasher);
    }

    [Fact]
    public async Task Handle_UniqueEmailAndUsername_HashesPasswordAndPersistsUser()
    {
        // Arrange — the fakes return false for the existence checks by default
        // (an unconfigured Task<bool> substitute yields false), so the user is unique.
        _passwordHasher.Hash("supersecret123").Returns("HASHED");
        var command = new RegisterUserCommand("alice", "alice@example.com", "supersecret123");

        // Act
        var result = await _sut.Handle(command);

        // Assert — success, and the response echoes the new user
        Assert.True(result.IsSuccess);
        Assert.Equal("alice", result.Value.Username);
        Assert.Equal("alice@example.com", result.Value.Email);

        // The user was persisted with the HASHED password — never the plaintext.
        // (u is null-guarded because an argument matcher is, in principle, null-tolerant.)
        await _users.Received(1).AddAsync(
            Arg.Is<User>(u =>
                u != null &&
                u.Username == "alice" &&
                u.Email == "alice@example.com" &&
                u.PasswordHash == "HASHED"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailAlreadyTaken_ReturnsConflict_AndDoesNotPersist()
    {
        // Arrange — the repository reports the email as already taken
        _users.EmailExistsAsync("taken@example.com", Arg.Any<CancellationToken>()).Returns(true);
        var command = new RegisterUserCommand("bob", "taken@example.com", "supersecret123");

        // Act
        var result = await _sut.Handle(command);

        // Assert — a Conflict error, and NOTHING was written or hashed
        Assert.True(result.IsFailure);
        Assert.Equal("User.EmailNotUnique", result.Error.Code);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);

        await _users.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        _passwordHasher.DidNotReceive().Hash(Arg.Any<string>());
    }
}
