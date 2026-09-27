using Lash.Users.Messaging.Events;

namespace Lash.Users.Application.Abstractions;

public interface IUsersEventPublisher
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : class, IUsersEvent;
}
