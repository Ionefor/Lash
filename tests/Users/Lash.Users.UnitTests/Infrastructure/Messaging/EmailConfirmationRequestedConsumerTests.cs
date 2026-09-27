using CSharpFunctionalExtensions;
using ErrorsFlow;
using ErrorsFlow.Models;
using Lash.Users.Infrastructure.Messaging;
using Lash.Users.Infrastructure.Providers;
using Lash.Users.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Lash.Users.UnitTests.Infrastructure.Messaging;

public sealed class EmailConfirmationRequestedConsumerTests
{
    [Fact]
    public async Task Consume_WhenUserExistsAndEmailIsNotConfirmed_SendsConfirmationEmail()
    {
        var userId = Guid.NewGuid();
        var sender = new Mock<IEmailConfirmationEmailSender>();
        sender.Setup(item => item.SendAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(UnitResult.Success<Error>());
        var consumer = new EmailConfirmationRequestedConsumer(
            sender.Object,
            NullLogger<EmailConfirmationRequestedConsumer>.Instance);

        await consumer.Consume(CreateContext(userId));

        sender.Verify(item => item.SendAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_WhenEmailCannotBeSent_ThrowsToTriggerMassTransitRetry()
    {
        var userId = Guid.NewGuid();
        var sender = new Mock<IEmailConfirmationEmailSender>();
        sender.Setup(item => item.SendAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(UnitResult.Failure(ErrorFactory.Create("email.failed", "Failed", ErrorType.Failure)));
        var consumer = new EmailConfirmationRequestedConsumer(
            sender.Object,
            NullLogger<EmailConfirmationRequestedConsumer>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => consumer.Consume(CreateContext(userId)));
    }

    private static ConsumeContext<EmailConfirmationRequested> CreateContext(Guid userId)
    {
        var context = new Mock<ConsumeContext<EmailConfirmationRequested>>();
        context.SetupGet(item => item.Message).Returns(new EmailConfirmationRequested(Guid.NewGuid(), userId, DateTimeOffset.UtcNow));
        return context.Object;
    }
}
