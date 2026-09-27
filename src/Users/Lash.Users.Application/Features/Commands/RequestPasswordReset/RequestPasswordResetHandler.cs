using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using FluentValidation;
using Lash.Users.Contracts.Events;
using Microsoft.Extensions.Logging;
using WebFlow.Abstractions.Interfaces;
using WebFlow.FluentValidation.Extensions;

namespace Lash.Users.Application.Features.Commands.RequestPasswordReset;

public sealed class RequestPasswordResetHandler(
    IValidator<RequestPasswordResetCommand> validator,
    IUserAccountService accounts,
    IUsersEventPublisher eventPublisher,
    IIdentityEmailRequestLimiter limiter,
    IUnitOfWork unitOfWork,
    ILogger<RequestPasswordResetHandler> logger,
    TimeProvider timeProvider) : ICommandHandler<RequestPasswordResetCommand>
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
            logger.LogWarning("Password reset request was rate limited.");
            return UnitResult.Success<ErrorList>();
        }

        var user = await accounts.FindByEmailAsync(command.Email, cancellationToken);
        if (user is not null)
        {
            await eventPublisher.PublishAsync(new PasswordResetRequested(
                Guid.NewGuid(),
                user.Id,
                timeProvider.GetUtcNow()), cancellationToken);
            logger.LogInformation("Password reset requested for user {UserId}.", user.Id);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<ErrorList>();
    }
}
