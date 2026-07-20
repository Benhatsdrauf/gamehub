using FluentValidation;

namespace GameHub.Application.Users.RegisterUser;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(command => command.Username)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(command => command.Password)
            .NotEmpty()
            .MinimumLength(8);
    }
}
