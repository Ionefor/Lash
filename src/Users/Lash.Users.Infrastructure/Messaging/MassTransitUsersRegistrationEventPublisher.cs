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
    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : class, IUsersEvent
    {
        await publishEndpoint.Publish(integrationEvent, cancellationToken);
        logger.LogInformation("Users event {EventId} published for user {UserId}.", integrationEvent.EventId, integrationEvent.UserId);
    }
}
