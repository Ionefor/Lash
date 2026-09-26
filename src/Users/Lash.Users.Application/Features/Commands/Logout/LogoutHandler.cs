using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application.Features.Commands.Logout;

public sealed class LogoutHandler(
    IRefreshSessionManager refreshSessionManager,
    IUnitOfWork unitOfWork) : ICommandHandler<LogoutCommand>
{
    public async Task<UnitResult<ErrorList>> Handle(LogoutCommand command, CancellationToken cancellationToken = default)
    {
        var session = await refreshSessionManager.GetByRefreshTokenAsync(command.RefreshToken, cancellationToken);
        if (session.IsFailure)
            return session.Error.ToErrorList();
        var revoke = session.Value.Revoke(DateTimeOffset.UtcNow);
        if (revoke.IsFailure)
            return revoke.Error.ToErrorList();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<ErrorList>();
    }
}
