using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using FluentValidation;
using Lash.Users.Application.Extensions;
using Lash.Users.Application.Errors;
using Microsoft.Extensions.Logging;
using WebFlow.Abstractions.Interfaces;
using WebFlow.FluentValidation.Extensions;

namespace Lash.Users.Application.Features.Commands.ResetPassword;

public sealed class ResetPasswordHandler(
    IValidator<ResetPasswordCommand> validator,
    IUserAccountService accounts,
    IRefreshSessionManager refreshSessionManager,
    IUnitOfWork unitOfWork,
    IUserSessionLock userSessionLock,
    ILogger<ResetPasswordHandler> logger) : ICommandHandler<ResetPasswordCommand>
{
    public async Task<UnitResult<ErrorList>> Handle(ResetPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            logger.LogWarning("Password reset validation failed.");
            return validation.ToErrorList();
        }

        var user = await accounts.FindByEmailAsync(command.Email, cancellationToken);
        if (user is null)
        {
            logger.LogDebug("Password reset was requested for an unknown account.");
            return UsersApplicationErrors.PasswordResetCodeInvalid().ToErrorList();
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        if (!await userSessionLock.TryAcquireAsync(user.Id, cancellationToken))
        {
            return UsersApplicationErrors.PasswordResetCodeInvalid().ToErrorList();
        }
        var reset = await accounts.ResetPasswordAsync(user.Id, command.Code, command.Password, cancellationToken);
        if (reset.IsFailure)
        {
            logger.LogDebug("Password reset failed for user {UserId}.", user.Id);
            return reset.Error.ToErrorList();
        }

        await refreshSessionManager.RevokeAllForUserAsync(user.Id, DateTimeOffset.UtcNow, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Password reset completed for user {UserId}.", user.Id);
        return UnitResult.Success<ErrorList>();
    }
}
