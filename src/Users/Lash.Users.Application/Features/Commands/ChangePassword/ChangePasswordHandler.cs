using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using FluentValidation;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Extensions;
using Microsoft.Extensions.Logging;
using WebFlow.Abstractions.Interfaces;
using WebFlow.FluentValidation.Extensions;

namespace Lash.Users.Application.Features.Commands.ChangePassword;

public sealed class ChangePasswordHandler(
    IValidator<ChangePasswordCommand> validator,
    IUserAccountService accounts,
    IRefreshSessionManager refreshSessionManager,
    IUnitOfWork unitOfWork,
    IUserSessionLock userSessionLock,
    ILogger<ChangePasswordHandler> logger,
    TimeProvider timeProvider) : ICommandHandler<ChangePasswordCommand>
{
    public async Task<UnitResult<ErrorList>> Handle(ChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            logger.LogWarning("Password change validation failed.");
            return validation.ToErrorList();
        }

        var user = await accounts.FindByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            logger.LogWarning("Password change was requested for an unknown user {UserId}.", command.UserId);
            return AuthErrors.CredentialsInvalid().ToErrorList();
        }
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        if (!await userSessionLock.TryAcquireAsync(user.Id, cancellationToken))
        {
            return AuthErrors.CredentialsInvalid().ToErrorList();
        }
        var change = await accounts.ChangePasswordAsync(user.Id, command.CurrentPassword, command.Password, cancellationToken);
        if (change.IsFailure)
        {
            logger.LogWarning("Password change failed for user {UserId}.", user.Id);
            return change.Error.ToErrorList();
        }

        await refreshSessionManager.RevokeAllForUserAsync(user.Id, timeProvider.GetUtcNow(), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Password changed for user {UserId}.", user.Id);
        return UnitResult.Success<ErrorList>();
    }
}
