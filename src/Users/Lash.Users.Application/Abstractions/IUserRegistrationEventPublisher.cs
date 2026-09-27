using Lash.Users.Messaging.Events;

namespace Lash.Users.Application.Abstractions;

public interface IUsersEventPublisher
{
    Task PublishAsync(ClientRegistered integrationEvent, CancellationToken cancellationToken = default);

    Task PublishAsync(MasterRegistered integrationEvent, CancellationToken cancellationToken = default);

    Task PublishAsync(EmailConfirmationRequested integrationEvent, CancellationToken cancellationToken = default);
}
