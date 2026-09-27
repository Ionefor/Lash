using Lash.Users.Application.Abstractions;
using Lash.Users.Contracts.Events;
using Lash.Users.Infrastructure.Providers;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Lash.Users.Infrastructure.Messaging;

public sealed class PasswordResetRequestedConsumer(
    IPasswordResetSender passwordResetSender,
    ILogger<PasswordResetRequestedConsumer> logger)
    : IConsumer<PasswordResetRequested>
{
    public async Task Consume(ConsumeContext<PasswordResetRequested> context)
    {
        var send = await passwordResetSender.SendAsync(context.Message.UserId, context.CancellationToken);
        if (send.IsFailure)
        {
            var exception = new InvalidOperationException("Unable to send password reset email.");
            logger.LogError(exception, "Password reset delivery failed for user {UserId}.", context.Message.UserId);
            throw exception;
        }
    }
}
