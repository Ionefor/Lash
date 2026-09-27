using FluentValidation;
using Lash.Users.Application.Extensions;

namespace Lash.Users.Application.Features.Commands.RegisterClient;

public sealed class RegisterClientCommandValidator : AbstractValidator<RegisterClientCommand>
{
    public RegisterClientCommandValidator()
    {
        RuleFor(command => command.Email).MustBeValidEmail();

        RuleFor(command => command.Password)
            .MustBeStrongPassword();

        RuleFor(command => command.ConfirmPassword)
            .MustMatchPassword(command => command.Password);
    }
}
