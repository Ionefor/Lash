using FluentValidation;
using Lash.Users.Application.Extensions;

namespace Lash.Users.Application.Features.Commands.RegisterMaster;

public sealed class RegisterMasterCommandValidator : AbstractValidator<RegisterMasterCommand>
{
    public RegisterMasterCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(command => command.Password)
            .MustBeStrongPassword();

        RuleFor(command => command.ConfirmPassword)
            .MustMatchPassword(command => command.Password);
    }
}
