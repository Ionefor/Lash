using CSharpFunctionalExtensions;
using ErrorsFlow;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Infrastructure.Messaging;
using Lash.Users.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Lash.Users.UnitTests.Infrastructure.Messaging;

public sealed class PasswordResetRequestedConsumerTests
{
    [Fact]
    public async Task Consume_WhenEmailIsSent_CompletesSuccessfully()
    {
        var userId = Guid.NewGuid();
        var sender = new Mock<IPasswordResetSender>();
        sender.Setup(item => item.SendAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(UnitResult.Success<Error>());
        var consumer = new PasswordResetRequestedConsumer(
            sender.Object,
            NullLogger<PasswordResetRequestedConsumer>.Instance);

        await consumer.Consume(CreateContext(userId));

        sender.Verify(item => item.SendAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_WhenEmailCannotBeSent_ThrowsToTriggerMassTransitRetry()
    {
        var userId = Guid.NewGuid();
        var sender = new Mock<IPasswordResetSender>();
        sender.Setup(item => item.SendAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(UnitResult.Failure(ErrorFactory.Create("email.failed", "Failed", ErrorType.Failure)));
        var consumer = new PasswordResetRequestedConsumer(
            sender.Object,
            NullLogger<PasswordResetRequestedConsumer>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => consumer.Consume(CreateContext(userId)));
    }

    private static ConsumeContext<PasswordResetRequested> CreateContext(Guid userId)
    {
        var context = new Mock<ConsumeContext<PasswordResetRequested>>();
        context.SetupGet(item => item.Message).Returns(new PasswordResetRequested(Guid.NewGuid(), userId, DateTimeOffset.UtcNow));
        return context.Object;
    }
}
