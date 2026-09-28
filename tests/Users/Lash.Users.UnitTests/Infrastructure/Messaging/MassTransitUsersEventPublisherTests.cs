using Lash.Users.Contracts.Events;
using Lash.Users.Infrastructure.Messaging;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Lash.Users.UnitTests.Infrastructure.Messaging;

public sealed class MassTransitUsersEventPublisherTests
{
    [Fact]
    public async Task PublishAsync_WhenEventIsProvided_PublishesItWithMassTransit()
    {
        var integrationEvent = new EmailConfirmationRequested(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
        var publishEndpoint = new Mock<IPublishEndpoint>();
        publishEndpoint.Setup(endpoint => endpoint.Publish(
                integrationEvent,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var publisher = new MassTransitUsersEventPublisher(
            publishEndpoint.Object,
            NullLogger<MassTransitUsersEventPublisher>.Instance);

        await publisher.PublishAsync(integrationEvent);

        publishEndpoint.Verify(endpoint => endpoint.Publish(
            integrationEvent,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
