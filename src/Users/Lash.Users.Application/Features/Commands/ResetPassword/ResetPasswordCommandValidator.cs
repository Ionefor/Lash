using FluentValidation;
using Lash.Users.Application.Extensions;
using Lash.Users.Application.Errors;

namespace Lash.Users.Application.Features.Commands.ResetPassword;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(command => command.Email).MustBeValidEmail();
        RuleFor(command => command.Code)
            .NotEmpty()
            .WithErrorCode(UsersValidationErrorCodes.PasswordResetCodeRequired)
            .WithMessage("Password reset code must be provided.");
        RuleFor(command => command.Password).MustBeStrongPassword();
        RuleFor(command => command.ConfirmPassword).MustMatchPassword(command => command.Password);
    }
}
