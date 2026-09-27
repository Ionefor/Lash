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
        var handler = new LogoutHandler(refreshSessions.Object, unitOfWork.Object, NullLogger<LogoutHandler>.Instance);

        var result = await handler.Handle(new LogoutCommand("refresh"));

        Assert.True(result.IsFailure);
        Assert.Equal(AuthErrorCodes.RefreshTokenInvalid, result.Error[0].Code);
        unitOfWork.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRefreshSessionIsActive_RevokesAndSavesChanges()
    {
        var session = CreateSession();
        var refreshSessions = new Mock<IRefreshSessionManager>();
        refreshSessions.Setup(item => item.GetByRefreshTokenAsync("refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<RefreshSession, Error>(session));
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var handler = new LogoutHandler(refreshSessions.Object, unitOfWork.Object, NullLogger<LogoutHandler>.Instance);

        var result = await handler.Handle(new LogoutCommand("refresh"));

        Assert.True(result.IsSuccess);
        Assert.NotNull(session.RevokedAt);
        unitOfWork.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static RefreshSession CreateSession()
    {
        var session = RefreshSession.Create(Guid.NewGuid(), Guid.NewGuid(), "hash", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1));
        Assert.True(session.IsSuccess);
        return session.Value;
    }
}
