using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Errors;
using Lash.Users.Application.Extensions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using WebFlow.Abstractions.Interfaces;
using WebFlow.FluentValidation.Extensions;

namespace Lash.Users.Application.Features.Commands.ConfirmEmail;

public sealed class ConfirmEmailHandler(
    IValidator<ConfirmEmailCommand> validator,
    IUserAccountService accounts,
    ILogger<ConfirmEmailHandler> logger) : ICommandHandler<ConfirmEmailCommand>
{
    public async Task<UnitResult<ErrorList>> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            logger.LogWarning("Email confirmation validation failed.");
            return validation.ToErrorList();
        }

        var user = await accounts.FindByEmailAsync(command.Email, cancellationToken);
        if (user is null)
        {
            logger.LogWarning("Email confirmation was requested for an unknown account.");
            return UsersApplicationErrors.EmailConfirmationCodeInvalid().ToErrorList();
        }

        var confirmation = await accounts.ConfirmEmailAsync(user.Id, command.Code, cancellationToken);
        if (confirmation.IsFailure)
        {
            logger.LogWarning("Email confirmation failed for user {UserId}.", user.Id);
            return confirmation.Error.ToErrorList();
        }

        logger.LogInformation("Email confirmed for user {UserId}.", user.Id);
        return UnitResult.Success<ErrorList>();
    }
}
