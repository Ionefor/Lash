using FluentValidation;
using Lash.Users.Application.Extensions;

namespace Lash.Users.Application.Features.Commands.RequestPasswordReset;

public sealed class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator() =>
        RuleFor(command => command.Email).MustBeValidEmail();
}
