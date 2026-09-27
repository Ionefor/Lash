using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Features.Commands.Logout;
using Lash.Users.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.UnitTests.Application.Features.Commands.Logout;

public sealed class LogoutHandlerTests
{
    [Fact]
    public async Task Handle_WhenRefreshSessionIsInvalid_DoesNotSaveChanges()
    {
        var refreshSessions = new Mock<IRefreshSessionManager>();
        refreshSessions.Setup(item => item.GetByRefreshTokenAsync("refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<RefreshSession, Error>(AuthErrors.RefreshTokenInvalid()));
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new LogoutHandler(refreshSessions.Object, unitOfWork.Object, CreateUserSessionLock().Object, NullLogger<LogoutHandler>.Instance, TimeProvider.System);

        var result = await handler.Handle(new LogoutCommand("refresh"));

        Assert.True(result.IsFailure);
        Assert.Equal(AuthErrorCodes.RefreshTokenInvalid, result.Error[0].Code);
        unitOfWork.Verify(item => item.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRefreshSessionIsActive_RevokesAndCommitsTransaction()
    {
        var session = CreateSession();
        var refreshSessions = new Mock<IRefreshSessionManager>();
        refreshSessions.Setup(item => item.GetByRefreshTokenAsync("refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<RefreshSession, Error>(session));
        var unitOfWork = new Mock<IUnitOfWork>();
        var transaction = new Mock<ITransaction>();
        unitOfWork.Setup(item => item.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transaction.Object);
        refreshSessions.Setup(item => item.TryRevokeAsync(session.Id, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = new LogoutHandler(refreshSessions.Object, unitOfWork.Object, CreateUserSessionLock().Object, NullLogger<LogoutHandler>.Instance, TimeProvider.System);

        var result = await handler.Handle(new LogoutCommand("refresh"));

        Assert.True(result.IsSuccess);
        refreshSessions.Verify(item => item.TryRevokeAsync(session.Id, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
        transaction.Verify(item => item.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static RefreshSession CreateSession()
    {
        var createdAt = new DateTimeOffset(2026, 9, 27, 0, 0, 0, TimeSpan.Zero);
        var session = RefreshSession.Create(Guid.NewGuid(), Guid.NewGuid(), "hash", createdAt, createdAt.AddDays(1));
        Assert.True(session.IsSuccess);
        return session.Value;
    }

    private static Mock<IUserSessionLock> CreateUserSessionLock()
    {
        var userSessionLock = new Mock<IUserSessionLock>();
        userSessionLock.Setup(item => item.TryAcquireAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        return userSessionLock;
    }
}
