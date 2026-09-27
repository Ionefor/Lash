using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Features.Commands.ChangePassword;
using Lash.Users.Application.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.UnitTests.Application.Features.Commands.ChangePassword;

public sealed class ChangePasswordHandlerTests
{
    [Fact]
    public async Task Handle_WhenCommandIsInvalid_DoesNotAccessAccount()
    {
        var accounts = new Mock<IUserAccountService>();
        var handler = new ChangePasswordHandler(new ChangePasswordCommandValidator(), accounts.Object, Mock.Of<IRefreshSessionManager>(), Mock.Of<IUnitOfWork>(), CreateUserSessionLock().Object, NullLogger<ChangePasswordHandler>.Instance, TimeProvider.System);

        var result = await handler.Handle(new ChangePasswordCommand(Guid.Empty, "", "Password1!", "Password1!"));

        Assert.True(result.IsFailure);
        accounts.Verify(item => item.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPasswordChangeSucceeds_ReturnsSuccess()
    {
        var user = new UserAccount(Guid.NewGuid(), "user@example.com", true);
        var accounts = new Mock<IUserAccountService>();
        accounts.Setup(item => item.FindByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        accounts.Setup(item => item.ChangePasswordAsync(user.Id, "Current1!", "Password1!", It.IsAny<CancellationToken>())).ReturnsAsync(UnitResult.Success<Error>());
        var refreshSessions = new Mock<IRefreshSessionManager>();
        var transaction = new Mock<ITransaction>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(item => item.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transaction.Object);
        var handler = new ChangePasswordHandler(new ChangePasswordCommandValidator(), accounts.Object, refreshSessions.Object, unitOfWork.Object, CreateUserSessionLock().Object, NullLogger<ChangePasswordHandler>.Instance, TimeProvider.System);

        var result = await handler.Handle(new ChangePasswordCommand(user.Id, "Current1!", "Password1!", "Password1!"));

        Assert.True(result.IsSuccess);
        accounts.Verify(item => item.ChangePasswordAsync(user.Id, "Current1!", "Password1!", It.IsAny<CancellationToken>()), Times.Once);
        refreshSessions.Verify(item => item.RevokeAllForUserAsync(user.Id, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
        transaction.Verify(item => item.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCurrentPasswordIsInvalid_DoesNotRevokeSessions()
    {
        var user = new UserAccount(Guid.NewGuid(), "user@example.com", true);
        var accounts = new Mock<IUserAccountService>();
        accounts.Setup(item => item.FindByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        accounts.Setup(item => item.ChangePasswordAsync(user.Id, "Wrong1!", "Password1!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(UnitResult.Failure(AuthErrors.CredentialsInvalid()));
        var refreshSessions = new Mock<IRefreshSessionManager>();
        var transaction = new Mock<ITransaction>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(item => item.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transaction.Object);
        var handler = new ChangePasswordHandler(new ChangePasswordCommandValidator(), accounts.Object, refreshSessions.Object, unitOfWork.Object, CreateUserSessionLock().Object, NullLogger<ChangePasswordHandler>.Instance, TimeProvider.System);

        var result = await handler.Handle(new ChangePasswordCommand(user.Id, "Wrong1!", "Password1!", "Password1!"));

        Assert.True(result.IsFailure);
        refreshSessions.Verify(item => item.RevokeAllForUserAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
        transaction.Verify(item => item.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Mock<IUserSessionLock> CreateUserSessionLock()
    {
        var userSessionLock = new Mock<IUserSessionLock>();
        userSessionLock.Setup(item => item.TryAcquireAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        return userSessionLock;
    }
}
