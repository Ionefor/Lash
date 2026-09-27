using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using FluentValidation;
using Lash.Users.Contracts.Events;
using Microsoft.Extensions.Logging;
using WebFlow.Abstractions.Interfaces;
using WebFlow.FluentValidation.Extensions;

namespace Lash.Users.Application.Features.Commands.ResendEmailConfirmation;

public sealed class ResendEmailConfirmationHandler(
    IValidator<ResendEmailConfirmationCommand> validator,
    IUserAccountService accounts,
    IUsersEventPublisher eventPublisher,
    IIdentityEmailRequestLimiter limiter,
    IUnitOfWork unitOfWork,
    ILogger<ResendEmailConfirmationHandler> logger,
    TimeProvider timeProvider) : ICommandHandler<ResendEmailConfirmationCommand>
{
    public async Task<UnitResult<ErrorList>> Handle(ResendEmailConfirmationCommand command, CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            logger.LogWarning("Email confirmation resend validation failed.");
            return validation.ToErrorList();
        }

        if (!await limiter.TryAcquireAsync(command.Email, IdentityEmailOperation.EmailConfirmation, cancellationToken))
        {
            logger.LogWarning("Email confirmation resend was rate limited.");
            return UnitResult.Success<ErrorList>();
        }

        var user = await accounts.FindByEmailAsync(command.Email, cancellationToken);
        if (user is not null && !user.EmailConfirmed)
        {
            await eventPublisher.PublishAsync(new EmailConfirmationRequested(
                Guid.NewGuid(),
                user.Id,
                timeProvider.GetUtcNow()), cancellationToken);
            logger.LogInformation("Email confirmation resend requested for user {UserId}.", user.Id);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<ErrorList>();
    }
}
