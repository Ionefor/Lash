using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Features.Commands.ResendEmailConfirmation;
using Lash.Users.Application.Models;
using Lash.Users.Messaging.Events;
using Moq;
using WebFlow.Abstractions.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lash.Users.UnitTests.Application.Features.Commands.ResendEmailConfirmation;

public sealed class ResendEmailConfirmationHandlerTests
{
    [Fact]
    public async Task Handle_WhenRequestLimitIsReached_DoesNotPublishEvent()
    {
        var accounts = new Mock<IUserAccountService>();
        var publisher = new Mock<IUsersEventPublisher>();
        var limiter = new Mock<IIdentityEmailRequestLimiter>();
        limiter.Setup(item => item.TryAcquireAsync("user@example.com", IdentityEmailOperation.EmailConfirmation, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new ResendEmailConfirmationHandler(
            new ResendEmailConfirmationCommandValidator(),
            accounts.Object,
            publisher.Object,
            limiter.Object,
            unitOfWork.Object,
            NullLogger<ResendEmailConfirmationHandler>.Instance);

        var result = await handler.Handle(new ResendEmailConfirmationCommand("user@example.com"));

        Assert.True(result.IsSuccess);
        publisher.Verify(item => item.PublishAsync(It.IsAny<EmailConfirmationRequested>(), It.IsAny<CancellationToken>()), Times.Never);
        accounts.Verify(item => item.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAccountIsUnconfirmed_StoresConfirmationEvent()
    {
        var userId = Guid.NewGuid();
        var accounts = new Mock<IUserAccountService>();
        accounts.Setup(item => item.FindByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAccount(userId, "user@example.com", false));
        var publisher = new Mock<IUsersEventPublisher>();
        publisher.Setup(item => item.PublishAsync(It.IsAny<EmailConfirmationRequested>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var limiter = new Mock<IIdentityEmailRequestLimiter>();
        limiter.Setup(item => item.TryAcquireAsync("user@example.com", IdentityEmailOperation.EmailConfirmation, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var handler = new ResendEmailConfirmationHandler(
            new ResendEmailConfirmationCommandValidator(),
            accounts.Object,
            publisher.Object,
            limiter.Object,
            unitOfWork.Object,
            NullLogger<ResendEmailConfirmationHandler>.Instance);

        var result = await handler.Handle(new ResendEmailConfirmationCommand("user@example.com"));

        Assert.True(result.IsSuccess);
        publisher.Verify(item => item.PublishAsync(It.Is<EmailConfirmationRequested>(message => message.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
