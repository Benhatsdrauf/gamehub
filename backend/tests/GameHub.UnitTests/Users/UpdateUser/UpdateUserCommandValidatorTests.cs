using GameHub.Application.Users.UpdateUser;

namespace GameHub.UnitTests.Users.UpdateUser;

public class UpdateUserCommandValidatorTests
{
    private readonly UpdateUserCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var command = new UpdateUserCommand(Guid.NewGuid(), "alice", "alice@example.com");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_BlankUsernameAndBadEmail_ReportsBothFields()
    {
        var command = new UpdateUserCommand(Guid.NewGuid(), "", "not-an-email");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateUserCommand.Username));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateUserCommand.Email));
    }
}
