using FluentValidation;
using Lash.Users.Application.Extensions;

namespace Lash.Users.Application.Features.Commands.ResendEmailConfirmation;

public sealed class ResendEmailConfirmationCommandValidator : AbstractValidator<ResendEmailConfirmationCommand>
{
    public ResendEmailConfirmationCommandValidator() =>
        RuleFor(command => command.Email).MustBeValidEmail();
}
