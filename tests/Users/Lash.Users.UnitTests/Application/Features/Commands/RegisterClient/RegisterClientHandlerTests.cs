using CSharpFunctionalExtensions;
using ErrorsFlow;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Constants;
using Lash.Users.Application.Features.Commands.RegisterClient;
using Lash.Users.Application.Models;
using Lash.Users.Messaging.Events;
using Moq;
using WebFlow.Abstractions.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lash.Users.UnitTests.Application.Features.Commands.RegisterClient;

public sealed class RegisterClientHandlerTests
{
    [Fact]
    public async Task Handle_WhenRegistrationSucceeds_CreatesAccountAndStoresEventsInTransaction()
    {
        var userId = Guid.NewGuid();
        var accounts = new Mock<IUserAccountService>();
        accounts.Setup(item => item.RoleExistsAsync(AccountRoleNames.Client, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        accounts.Setup(item => item.CreateAsync("client@example.com", "Password1!", AccountRoleNames.Client, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<UserAccount, Error>(new UserAccount(userId, "client@example.com", false)));
        var publisher = CreatePublisher();
        var unitOfWork = CreateUnitOfWork();
        var handler = new RegisterClientHandler(new RegisterClientCommandValidator(), accounts.Object, publisher.Object, unitOfWork.Object, NullLogger<RegisterClientHandler>.Instance);

        var result = await handler.Handle(new RegisterClientCommand("client@example.com", "Password1!", "Password1!"));

        Assert.True(result.IsSuccess);
        Assert.Equal(userId, result.Value);
        publisher.Verify(item => item.PublishAsync(It.Is<ClientRegistered>(message => message.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
        publisher.Verify(item => item.PublishAsync(It.Is<EmailConfirmationRequested>(message => message.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenClientRoleIsMissing_ReturnsRequiredRoleError()
    {
        var accounts = new Mock<IUserAccountService>();
        accounts.Setup(item => item.RoleExistsAsync(AccountRoleNames.Client, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new RegisterClientHandler(new RegisterClientCommandValidator(), accounts.Object, CreatePublisher().Object, CreateUnitOfWork().Object, NullLogger<RegisterClientHandler>.Instance);

        var result = await handler.Handle(new RegisterClientCommand("client@example.com", "Password1!", "Password1!"));

        Assert.True(result.IsFailure);
        Assert.Equal("users.required_role.not_configured", result.Error[0].Code);
        Assert.Equal(ErrorType.Failure, result.Error[0].Type);
        Assert.Equal("role", result.Error[0].Target);
    }

    private static Mock<IUsersEventPublisher> CreatePublisher()
    {
        var publisher = new Mock<IUsersEventPublisher>();
        publisher.Setup(item => item.PublishAsync(It.IsAny<ClientRegistered>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        publisher.Setup(item => item.PublishAsync(It.IsAny<EmailConfirmationRequested>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        return publisher;
    }

    private static Mock<IUnitOfWork> CreateUnitOfWork()
    {
        var transaction = new Mock<ITransaction>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(item => item.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transaction.Object);
        unitOfWork.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        return unitOfWork;
    }
}
