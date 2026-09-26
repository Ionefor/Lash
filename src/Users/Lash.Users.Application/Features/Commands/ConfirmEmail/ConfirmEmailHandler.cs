using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Application.Errors;
using Lash.Users.Application.Extensions;
using Lash.Users.Domain;
using Microsoft.AspNetCore.Identity;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application.Features.Commands.ConfirmEmail;

public sealed class ConfirmEmailHandler(UserManager<User> userManager) : ICommandHandler<ConfirmEmailCommand>
{
    public async Task<UnitResult<ErrorList>> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is null)
            return UsersApplicationErrors.EmailConfirmationCodeInvalid().ToErrorList();

        var confirmation = await userManager.ConfirmEmailAsync(user, command.Code);
        return confirmation.Succeeded
            ? UnitResult.Success<ErrorList>()
            : UsersApplicationErrors.EmailConfirmationCodeInvalid().ToErrorList();
    }
}
