using System.Net;
using System.Net.Mail;
using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Domain;
using Lash.Users.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Lash.Users.Infrastructure.Providers;

public sealed class EmailConfirmationSender(
    UserManager<User> userManager,
    IOptions<EmailOptions> options,
    ILogger<EmailConfirmationSender> logger) : IEmailConfirmationSender, IPasswordResetSender
{
    public async Task<UnitResult<Error>> SendAsync(User user, CancellationToken cancellationToken = default)
    {
        var email = user.Email;
        if (string.IsNullOrWhiteSpace(email)) return UnitResult.Failure(GeneralErrors.ValueIsRequired(nameof(user.Email)));
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        try
        {
            using var client = new SmtpClient(options.Value.Host, options.Value.Port) { EnableSsl = options.Value.UseSsl, Credentials = new NetworkCredential(options.Value.UserName, options.Value.Password) };
            using var message = new MailMessage(options.Value.FromAddress, email, "Код подтверждения Lash", $"Ваш код подтверждения: {code}");
            await client.SendMailAsync(message, cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (SmtpException)
        {
            logger.LogWarning("Unable to send email confirmation.");
            return UnitResult.Failure(GeneralErrors.Failed("Unable to send confirmation email."));
        }
    }
}

public sealed class PasswordResetSender(
    UserManager<User> userManager,
    IOptions<EmailOptions> options,
    ILogger<PasswordResetSender> logger) : IPasswordResetSender
{
    public async Task<UnitResult<Error>> SendAsync(User user, CancellationToken cancellationToken = default)
    {
        var email = user.Email;
        if (string.IsNullOrWhiteSpace(email))
            return UnitResult.Failure(GeneralErrors.ValueIsRequired(nameof(user.Email)));

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
            return UnitResult.Success<Error>();
        }
        catch (SmtpException)
        {
            logger.LogWarning("Unable to send password reset email.");
            return UnitResult.Failure(GeneralErrors.Failed("Unable to send password reset email."));
        }
    }
}
