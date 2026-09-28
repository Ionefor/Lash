using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Features.Commands.RequestPasswordReset;
using Lash.Users.Application.Models;
using Lash.Users.Contracts.Events;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.UnitTests.Application.Features.Commands.RequestPasswordReset;

public sealed class RequestPasswordResetHandlerTests
{
    [Fact]
    public async Task Handle_WhenCommandIsInvalid_DoesNotUseLimiter()
    {
        var limiter = new Mock<IIdentityEmailRequestLimiter>();
        var handler = new RequestPasswordResetHandler(
            new RequestPasswordResetCommandValidator(),
            Mock.Of<IUserAccountService>(),
            Mock.Of<IUsersEventPublisher>(),
            limiter.Object,
            Mock.Of<IUnitOfWork>(),
            NullLogger<RequestPasswordResetHandler>.Instance, TimeProvider.System);

        var result = await handler.Handle(new RequestPasswordResetCommand("invalid"));

        Assert.True(result.IsFailure);
        limiter.Verify(item => item.TryAcquireAsync(It.IsAny<string>(), It.IsAny<IdentityEmailOperation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRequestLimitIsReached_DoesNotPublishPasswordResetRequest()
    {
        var accounts = new Mock<IUserAccountService>();
        var publisher = new Mock<IUsersEventPublisher>();
        var limiter = new Mock<IIdentityEmailRequestLimiter>();
        limiter.Setup(item => item.TryAcquireAsync("user@example.com", IdentityEmailOperation.PasswordReset, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var unitOfWork = CreateUnitOfWork();
        var handler = new RequestPasswordResetHandler(new RequestPasswordResetCommandValidator(), accounts.Object, publisher.Object, limiter.Object, unitOfWork.Object, NullLogger<RequestPasswordResetHandler>.Instance, TimeProvider.System);

        var result = await handler.Handle(new RequestPasswordResetCommand("user@example.com"));

        Assert.True(result.IsSuccess);
        publisher.Verify(item => item.PublishAsync(It.IsAny<PasswordResetRequested>(), It.IsAny<CancellationToken>()), Times.Never);
        accounts.Verify(item => item.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAccountExists_PublishesPasswordResetRequestAndReturnsSuccess()
    {
        var user = new UserAccount(Guid.NewGuid(), "user@example.com", true);
        var accounts = new Mock<IUserAccountService>();
        accounts.Setup(item => item.FindByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var publisher = new Mock<IUsersEventPublisher>();
        var limiter = new Mock<IIdentityEmailRequestLimiter>();
        limiter.Setup(item => item.TryAcquireAsync("user@example.com", IdentityEmailOperation.PasswordReset, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var unitOfWork = CreateUnitOfWork();
        var handler = new RequestPasswordResetHandler(new RequestPasswordResetCommandValidator(), accounts.Object, publisher.Object, limiter.Object, unitOfWork.Object, NullLogger<RequestPasswordResetHandler>.Instance, TimeProvider.System);

        var result = await handler.Handle(new RequestPasswordResetCommand("user@example.com"));

        Assert.True(result.IsSuccess);
        publisher.Verify(item => item.PublishAsync(It.Is<PasswordResetRequested>(message => message.UserId == user.Id), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAccountDoesNotExist_DoesNotPublishPasswordResetRequest()
    {
        var accounts = new Mock<IUserAccountService>();
        var publisher = new Mock<IUsersEventPublisher>();
        var limiter = new Mock<IIdentityEmailRequestLimiter>();
        limiter.Setup(item => item.TryAcquireAsync("unknown@example.com", IdentityEmailOperation.PasswordReset, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var unitOfWork = CreateUnitOfWork();
        var handler = new RequestPasswordResetHandler(new RequestPasswordResetCommandValidator(), accounts.Object, publisher.Object, limiter.Object, unitOfWork.Object, NullLogger<RequestPasswordResetHandler>.Instance, TimeProvider.System);

        var result = await handler.Handle(new RequestPasswordResetCommand("unknown@example.com"));

        Assert.True(result.IsSuccess);
        publisher.Verify(item => item.PublishAsync(It.IsAny<PasswordResetRequested>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Mock<IUnitOfWork> CreateUnitOfWork()
    {
        var transaction = new Mock<ITransaction>();
        transaction.Setup(item => item.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(item => item.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction.Object);
        unitOfWork.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        return unitOfWork;
    }
}
