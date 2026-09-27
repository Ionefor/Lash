using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Microsoft.Extensions.Logging;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application.Features.Commands.Logout;

public sealed class LogoutHandler(
    IRefreshSessionManager refreshSessionManager,
    IUnitOfWork unitOfWork,
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
        var revoke = session.Value.Revoke(DateTimeOffset.UtcNow);
        if (revoke.IsFailure)
        {
            logger.LogDebug("Logout failed because refresh session could not be revoked.");
            return revoke.Error.ToErrorList();
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Logout completed for user {UserId}.", session.Value.UserId);
        return UnitResult.Success<ErrorList>();
    }
}
