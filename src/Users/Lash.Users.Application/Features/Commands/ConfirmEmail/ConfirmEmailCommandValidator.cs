using FluentValidation;
using Lash.Users.Application.Extensions;
using Lash.Users.Application.Errors;

namespace Lash.Users.Application.Features.Commands.ConfirmEmail;

public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(command => command.Email).MustBeValidEmail();
        RuleFor(command => command.Code)
            .NotEmpty()
            .WithErrorCode(UsersValidationErrorCodes.EmailConfirmationCodeRequired)
            .WithMessage("Email confirmation code must be provided.");
    }
}
