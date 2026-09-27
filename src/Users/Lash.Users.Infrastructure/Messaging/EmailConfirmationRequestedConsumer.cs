using Lash.Users.Messaging.Events;
using Lash.Users.Infrastructure.Providers;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Lash.Users.Infrastructure.Messaging;

public sealed class EmailConfirmationRequestedConsumer(
    IEmailConfirmationEmailSender emailConfirmationSender,
    ILogger<EmailConfirmationRequestedConsumer> logger)
    : IConsumer<EmailConfirmationRequested>
{
    public async Task Consume(ConsumeContext<EmailConfirmationRequested> context)
    {
        var send = await emailConfirmationSender.SendAsync(context.Message.UserId, context.CancellationToken);
        if (send.IsFailure)
        {
            var exception = new InvalidOperationException("Unable to send email confirmation.");
            logger.LogError(exception, "Email confirmation delivery failed for user {UserId}.", context.Message.UserId);
            throw exception;
        }

    }
}
