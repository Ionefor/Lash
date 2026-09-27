using System.Net;
using System.Net.Mail;
using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Errors;
using Lash.Users.Infrastructure.Identity;
using Lash.Users.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Lash.Users.Infrastructure.Providers;

public interface IEmailConfirmationEmailSender
{
    Task<UnitResult<Error>> SendAsync(Guid userId, CancellationToken cancellationToken = default);
}

public sealed class EmailConfirmationSender(
    UserManager<IdentityUserEntity> userManager,
    IOptions<EmailOptions> options,
    ILogger<EmailConfirmationSender> logger) : IEmailConfirmationEmailSender
{
    public async Task<UnitResult<Error>> SendAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.EmailConfirmed) return UnitResult.Success<Error>();
        var email = user.Email;
        if (string.IsNullOrWhiteSpace(email)) return UnitResult.Failure(GeneralErrors.ValueIsRequired("email"));
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        try
        {
            using var client = new SmtpClient(options.Value.Host, options.Value.Port) { EnableSsl = options.Value.UseSsl, Credentials = new NetworkCredential(options.Value.UserName, options.Value.Password) };
            using var message = new MailMessage(options.Value.FromAddress, email, "Код подтверждения Lash", $"Ваш код подтверждения: {code}");
            await client.SendMailAsync(message, cancellationToken);
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
    IOptions<EmailOptions> options,
    ILogger<PasswordResetSender> logger) : IPasswordResetSender
{
    public async Task<UnitResult<Error>> SendAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return UnitResult.Success<Error>();
        var email = user.Email;
        if (string.IsNullOrWhiteSpace(email))
            return UnitResult.Failure(GeneralErrors.ValueIsRequired("email"));

        var code = await userManager.GeneratePasswordResetTokenAsync(user);
        try
        {
            using var client = new SmtpClient(options.Value.Host, options.Value.Port)
            {
                EnableSsl = options.Value.UseSsl,
                Credentials = new NetworkCredential(options.Value.UserName, options.Value.Password)
            };
            using var message = new MailMessage(options.Value.FromAddress, email, "Password reset Lash", $"Your password reset code: {code}");
            await client.SendMailAsync(message, cancellationToken);
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
