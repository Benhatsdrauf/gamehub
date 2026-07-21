using FluentValidation;

namespace GameHub.Application.Authentication.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        // Login validation only asks "are the fields present and plausibly shaped?"
        // NOT "is this a strong password?" — password policy lives at registration.
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(command => command.Password)
            .NotEmpty();
    }
}
