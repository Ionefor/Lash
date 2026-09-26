using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Domain;
using Microsoft.AspNetCore.Identity;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application.Features.Commands.RequestPasswordReset;

public sealed class RequestPasswordResetHandler(
    UserManager<User> userManager,
    IPasswordResetSender sender,
    IIdentityEmailRequestLimiter limiter,
    IUnitOfWork unitOfWork) : ICommandHandler<RequestPasswordResetCommand>
{
    public async Task<UnitResult<ErrorList>> Handle(
        RequestPasswordResetCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!await limiter.TryAcquireAsync(command.Email, IdentityEmailOperation.PasswordReset, cancellationToken))
            return UnitResult.Success<ErrorList>();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is not null)
            await sender.SendAsync(user, cancellationToken);

        return UnitResult.Success<ErrorList>();
    }
}
