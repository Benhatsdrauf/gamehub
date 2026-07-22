using FluentValidation;

namespace GameHub.Application.Authentication.Logout;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(command => command.RefreshToken).NotEmpty();
    }
}
