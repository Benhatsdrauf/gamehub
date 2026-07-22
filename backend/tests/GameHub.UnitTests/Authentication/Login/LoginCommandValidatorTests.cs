using GameHub.Application.Authentication.Login;

namespace GameHub.UnitTests.Authentication.Login;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        // A well-formed email and any non-empty password. Note: login does NOT enforce
        // password length (that's a registration rule), so a short password is fine here.
        var result = _validator.Validate(new LoginCommand("alice@example.com", "x"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_BadEmailAndBlankPassword_ReportsBothFields()
    {
        var result = _validator.Validate(new LoginCommand("not-an-email", ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginCommand.Email));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginCommand.Password));
    }
}
