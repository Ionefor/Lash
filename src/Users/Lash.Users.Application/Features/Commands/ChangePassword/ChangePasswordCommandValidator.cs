using FluentValidation;
using Lash.Users.Application.Extensions;
using Lash.Users.Application.Errors;

namespace Lash.Users.Application.Features.Commands.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithErrorCode(UsersValidationErrorCodes.UserIdRequired)
            .WithMessage("User identifier must be provided.");
        RuleFor(command => command.CurrentPassword)
            .NotEmpty()
            .WithErrorCode(UsersValidationErrorCodes.CurrentPasswordRequired)
            .WithMessage("Current password must be provided.");
        RuleFor(command => command.Password).MustBeStrongPassword();
        RuleFor(command => command.ConfirmPassword).MustMatchPassword(command => command.Password);
    }
}
