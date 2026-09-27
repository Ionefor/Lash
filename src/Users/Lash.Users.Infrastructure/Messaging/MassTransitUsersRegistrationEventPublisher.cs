using Lash.Users.Application.Abstractions;
using Lash.Users.Messaging.Events;
using MassTransit;

namespace Lash.Users.Infrastructure.Messaging;

public sealed class MassTransitUsersRegistrationEventPublisher(IPublishEndpoint publishEndpoint)
    : IUsersEventPublisher
{
    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : class, IUsersEvent
    {
        await publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
