using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Constants;
using Lash.Users.Application.Features.Commands.RegisterMaster;
using Lash.Users.Application.Models;
using Lash.Users.Messaging.Events;
using Moq;
using WebFlow.Abstractions.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lash.Users.UnitTests.Application.Features.Commands.RegisterMaster;

public sealed class RegisterMasterHandlerTests
{
    [Fact]
    public async Task Handle_WhenRegistrationSucceeds_CreatesMasterAndStoresEventsInTransaction()
    {
        var userId = Guid.NewGuid();
        var accounts = new Mock<IUserAccountService>();
        accounts.Setup(item => item.RoleExistsAsync(AccountRoleNames.Master, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        accounts.Setup(item => item.CreateAsync("master@example.com", "Password1!", AccountRoleNames.Master, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<UserAccount, Error>(new UserAccount(userId, "master@example.com", false)));
        var publisher = new Mock<IUsersEventPublisher>();
        publisher.Setup(item => item.PublishAsync(It.IsAny<MasterRegistered>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        publisher.Setup(item => item.PublishAsync(It.IsAny<EmailConfirmationRequested>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var transaction = new Mock<ITransaction>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(item => item.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transaction.Object);
        unitOfWork.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var handler = new RegisterMasterHandler(new RegisterMasterCommandValidator(), accounts.Object, publisher.Object, unitOfWork.Object, NullLogger<RegisterMasterHandler>.Instance);

        var result = await handler.Handle(new RegisterMasterCommand("master@example.com", "Password1!", "Password1!"));

        Assert.True(result.IsSuccess);
        Assert.Equal(userId, result.Value);
        publisher.Verify(item => item.PublishAsync(It.Is<MasterRegistered>(message => message.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
        publisher.Verify(item => item.PublishAsync(It.Is<EmailConfirmationRequested>(message => message.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
