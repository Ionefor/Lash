using FluentValidation;
using Lash.Users.Application.Extensions;
using Lash.Users.Application.Errors;

namespace Lash.Users.Application.Features.Commands.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email).MustBeValidEmail();
        RuleFor(command => command.Password)
            .NotEmpty()
            .WithErrorCode(UsersValidationErrorCodes.PasswordRequired)
            .WithMessage("Password must be provided.");
    }
}
