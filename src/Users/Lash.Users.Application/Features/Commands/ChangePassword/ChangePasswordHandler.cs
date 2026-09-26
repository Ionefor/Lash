using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using FluentValidation;
using Lash.Users.Application.Extensions;
using Lash.Users.Domain;
using Microsoft.AspNetCore.Identity;
using WebFlow.Abstractions.Interfaces;
using WebFlow.FluentValidation.Extensions;

namespace Lash.Users.Application.Features.Commands.ChangePassword;

public sealed class ChangePasswordHandler(
    IValidator<ChangePasswordCommand> validator,
    UserManager<User> userManager) : ICommandHandler<ChangePasswordCommand>
{
    public async Task<UnitResult<ErrorList>> Handle(ChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            return validation.ToErrorList();

        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null || !await userManager.CheckPasswordAsync(user, command.CurrentPassword))
            return AuthErrors.CredentialsInvalid().ToErrorList();

        var change = await userManager.ChangePasswordAsync(user, command.CurrentPassword, command.Password);
        return change.Succeeded ? UnitResult.Success<ErrorList>() : change.ToErrorList();
    }
}
