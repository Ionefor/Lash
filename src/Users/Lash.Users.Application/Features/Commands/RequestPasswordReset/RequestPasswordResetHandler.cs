using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using WebFlow.Abstractions.Interfaces;
using WebFlow.FluentValidation.Extensions;

namespace Lash.Users.Application.Features.Commands.RequestPasswordReset;

public sealed class RequestPasswordResetHandler(
    IValidator<RequestPasswordResetCommand> validator,
    IUserAccountService accounts,
    IPasswordResetSender sender,
    IIdentityEmailRequestLimiter limiter,
    IUnitOfWork unitOfWork,
    ILogger<RequestPasswordResetHandler> logger) : ICommandHandler<RequestPasswordResetCommand>
{
    public async Task<UnitResult<ErrorList>> Handle(
        RequestPasswordResetCommand command,
        CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            logger.LogWarning("Password reset request validation failed.");
            return validation.ToErrorList();
        }

        if (!await limiter.TryAcquireAsync(command.Email, IdentityEmailOperation.PasswordReset, cancellationToken))
        {
            logger.LogDebug("Password reset request was rate limited.");
            return UnitResult.Success<ErrorList>();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var user = await accounts.FindByEmailAsync(command.Email, cancellationToken);
        if (user is not null)
        {
            var send = await sender.SendAsync(user.Id, cancellationToken);
            if (send.IsFailure)
                logger.LogWarning("Password reset email could not be sent for user {UserId}.", user.Id);
            else
                logger.LogInformation("Password reset email sent for user {UserId}.", user.Id);
        }

        logger.LogDebug("Password reset request completed.");

        return UnitResult.Success<ErrorList>();
    }
}
