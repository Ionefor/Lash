using System.Net.Mail;
using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Errors;
using Lash.Users.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Lash.Users.Infrastructure.Providers;

public interface IEmailConfirmationEmailSender
{
    Task<UnitResult<Error>> SendAsync(
        Guid userId,
        Guid eventId,
        CancellationToken cancellationToken = default);
}

public sealed class EmailConfirmationSender(
    UserManager<IdentityUserEntity> userManager,
    IIdentityEmailTemplateFactory templateFactory,
    IEmailTransport emailTransport,
    ILogger<EmailConfirmationSender> logger) : IEmailConfirmationEmailSender
{
    public async Task<UnitResult<Error>> SendAsync(
        Guid userId,
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.EmailConfirmed)
        {
            return UnitResult.Success<Error>();
        }

        var email = user.Email;
        if (string.IsNullOrWhiteSpace(email))
        {
            return UnitResult.Failure(GeneralErrors.ValueIsRequired("email"));
        }

        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        try
        {
            await emailTransport.SendAsync(
                templateFactory.CreateEmailConfirmation(email, code) with { EventId = eventId },
                cancellationToken);
            logger.LogInformation("Email confirmation sent for user {UserId}.", userId);
            return UnitResult.Success<Error>();
        }
        catch (SmtpException exception)
        {
            logger.LogWarning(exception, "Unable to send email confirmation for user {UserId}.", userId);
            return UnitResult.Failure(UsersApplicationErrors.EmailDeliveryFailed());
        }
    }
}

public sealed class PasswordResetSender(
    UserManager<IdentityUserEntity> userManager,
    IIdentityEmailTemplateFactory templateFactory,
    IEmailTransport emailTransport,
    ILogger<PasswordResetSender> logger) : IPasswordResetSender
{
    public async Task<UnitResult<Error>> SendAsync(
        Guid userId,
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return UnitResult.Success<Error>();
        }

        var email = user.Email;
        if (string.IsNullOrWhiteSpace(email))
        {
            return UnitResult.Failure(GeneralErrors.ValueIsRequired("email"));
        }

        var code = await userManager.GeneratePasswordResetTokenAsync(user);
        try
        {
            await emailTransport.SendAsync(
                templateFactory.CreatePasswordReset(email, code) with { EventId = eventId },
                cancellationToken);
            logger.LogInformation("Password reset email sent for user {UserId}.", userId);
            return UnitResult.Success<Error>();
        }
        catch (SmtpException exception)
        {
            logger.LogWarning(exception, "Unable to send password reset email for user {UserId}.", userId);
            return UnitResult.Failure(UsersApplicationErrors.EmailDeliveryFailed());
        }
    }
}
