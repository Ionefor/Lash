using Lash.Users.Application.Abstractions;
using Lash.Users.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Lash.Users.Infrastructure.Messaging;

public sealed class MassTransitUsersEventPublisher(
    IPublishEndpoint publishEndpoint,
    ILogger<MassTransitUsersEventPublisher> logger) : IUsersEventPublisher
{
    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : class, IUsersEvent
    {
        await publishEndpoint.Publish(integrationEvent, cancellationToken);
        logger.LogInformation(
            "Published Users integration event {EventType} {EventId}.",
            typeof(TEvent).Name,
            integrationEvent.EventId);
    }
}
