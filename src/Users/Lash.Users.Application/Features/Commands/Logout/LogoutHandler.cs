using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Microsoft.Extensions.Logging;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application.Features.Commands.Logout;

public sealed class LogoutHandler(
    IRefreshSessionManager refreshSessionManager,
    IUnitOfWork unitOfWork,
    IUserSessionLock userSessionLock,
    ILogger<LogoutHandler> logger) : ICommandHandler<LogoutCommand>
{
    public async Task<UnitResult<ErrorList>> Handle(LogoutCommand command, CancellationToken cancellationToken = default)
    {
        var session = await refreshSessionManager.GetByRefreshTokenAsync(command.RefreshToken, cancellationToken);
        if (session.IsFailure)
        {
            logger.LogDebug("Logout failed because refresh session was invalid.");
            return session.Error.ToErrorList();
        }
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        if (!await userSessionLock.TryAcquireAsync(session.Value.UserId, cancellationToken))
        {
            return AuthErrors.RefreshTokenInvalid().ToErrorList();
        }

        session = await refreshSessionManager.GetByRefreshTokenAsync(command.RefreshToken, cancellationToken);
        if (session.IsFailure || !await refreshSessionManager.TryRevokeAsync(session.Value.Id, DateTimeOffset.UtcNow, cancellationToken))
        {
            logger.LogDebug("Logout failed because refresh session could not be revoked.");
            return AuthErrors.RefreshTokenInvalid().ToErrorList();
        }

        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Logout completed for user {UserId}.", session.Value.UserId);
        return UnitResult.Success<ErrorList>();
    }
}
