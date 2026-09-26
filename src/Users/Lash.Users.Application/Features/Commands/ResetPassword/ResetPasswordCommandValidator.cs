using FluentValidation;
using Lash.Users.Application.Extensions;

namespace Lash.Users.Application.Features.Commands.ResetPassword;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
        RuleFor(command => command.Code).NotEmpty();
        RuleFor(command => command.Password).MustBeStrongPassword();
        RuleFor(command => command.ConfirmPassword).MustMatchPassword(command => command.Password);
    }
}
