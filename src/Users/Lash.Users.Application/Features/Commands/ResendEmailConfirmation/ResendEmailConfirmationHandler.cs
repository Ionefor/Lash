using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Domain;
using Microsoft.AspNetCore.Identity;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application.Features.Commands.ResendEmailConfirmation;

public sealed class ResendEmailConfirmationHandler(
    UserManager<User> userManager,
    IEmailConfirmationSender sender,
    IIdentityEmailRequestLimiter limiter,
    IUnitOfWork unitOfWork) : ICommandHandler<ResendEmailConfirmationCommand>
{
    public async Task<UnitResult<ErrorList>> Handle(ResendEmailConfirmationCommand command, CancellationToken cancellationToken = default)
    {
        if (!await limiter.TryAcquireAsync(command.Email, IdentityEmailOperation.EmailConfirmation, cancellationToken))
            return UnitResult.Success<ErrorList>();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is not null && !user.EmailConfirmed)
            await sender.SendAsync(user, cancellationToken);

        return UnitResult.Success<ErrorList>();
    }
}
