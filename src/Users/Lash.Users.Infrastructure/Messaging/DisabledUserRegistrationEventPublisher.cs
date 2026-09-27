using Lash.Users.Application.Abstractions;
using Lash.Users.Messaging.Events;

namespace Lash.Users.Infrastructure.Messaging;

internal sealed class DisabledUserRegistrationEventPublisher : IUsersEventPublisher
{
    public Task PublishAsync(ClientRegistered integrationEvent, CancellationToken cancellationToken = default) =>
        Task.FromException(new InvalidOperationException("RabbitMQ is disabled."));

    public Task PublishAsync(MasterRegistered integrationEvent, CancellationToken cancellationToken = default) =>
        Task.FromException(new InvalidOperationException("RabbitMQ is disabled."));

    public Task PublishAsync(EmailConfirmationRequested integrationEvent, CancellationToken cancellationToken = default) =>
        Task.FromException(new InvalidOperationException("RabbitMQ is disabled."));
}
