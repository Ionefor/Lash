using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using FluentValidation;
using Lash.Users.Application.Extensions;
using Lash.Users.Application.Errors;
using Lash.Users.Domain;
using Microsoft.AspNetCore.Identity;
using WebFlow.Abstractions.Interfaces;
using WebFlow.FluentValidation.Extensions;

namespace Lash.Users.Application.Features.Commands.ResetPassword;

public sealed class ResetPasswordHandler(
    IValidator<ResetPasswordCommand> validator,
    UserManager<User> userManager) : ICommandHandler<ResetPasswordCommand>
{
    public async Task<UnitResult<ErrorList>> Handle(ResetPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            return validation.ToErrorList();

        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is null)
            return UsersApplicationErrors.PasswordResetCodeInvalid().ToErrorList();

        var reset = await userManager.ResetPasswordAsync(user, command.Code, command.Password);
        if (reset.Succeeded)
            return UnitResult.Success<ErrorList>();

        return reset.Errors.Any(error => error.Code == nameof(IdentityErrorDescriber.InvalidToken))
            ? UsersApplicationErrors.PasswordResetCodeInvalid().ToErrorList()
            : reset.ToErrorList();
    }
}
