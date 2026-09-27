using Lash.Users.Application.Abstractions;
using Lash.Users.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Lash.Users.Infrastructure.Messaging;

public sealed class MassTransitUsersRegistrationEventPublisher(
    IPublishEndpoint publishEndpoint,
    ILogger<MassTransitUsersRegistrationEventPublisher> logger)
    : IUsersEventPublisher
{
    public async Task PublishAsync(ClientRegistered integrationEvent, CancellationToken cancellationToken = default)
    {
        await publishEndpoint.Publish(integrationEvent, cancellationToken);
        logger.LogInformation("Client registration event published for user {UserId}.", integrationEvent.UserId);
    }

    public async Task PublishAsync(MasterRegistered integrationEvent, CancellationToken cancellationToken = default)
    {
        await publishEndpoint.Publish(integrationEvent, cancellationToken);
        logger.LogInformation("Master registration event published for user {UserId}.", integrationEvent.UserId);
    }

    public async Task PublishAsync(EmailConfirmationRequested integrationEvent, CancellationToken cancellationToken = default)
    {
        await publishEndpoint.Publish(integrationEvent, cancellationToken);
        logger.LogInformation("Email confirmation request event published for user {UserId}.", integrationEvent.UserId);
    }
}
