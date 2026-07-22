using GameHub.Application.Users.RegisterUser;

namespace GameHub.UnitTests.Users.RegisterUser;

// Validators are the easiest thing to test: no dependencies, no mocks — construct
// it, feed it a command, inspect the result. (FluentValidation also ships a
// TestHelper with fluent sugar like ShouldHaveValidationErrorFor; we use plain
// Validate + Assert here to keep one assertion style and see exactly what it returns.)
public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_PassesWithNoErrors()
    {
        // Arrange
        var command = new RegisterUserCommand("alice", "alice@example.com", "supersecret123");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_BlankUsernameBadEmailShortPassword_ReportsEachField()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Username: "",
            Email: "not-an-email",
            Password: "short");

        // Act
        var result = _validator.Validate(command);

        // Assert — one failure per broken rule, keyed by the property name
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.Username));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.Email));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }
}
