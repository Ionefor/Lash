using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Features.Commands.ResetPassword;
using Lash.Users.Application.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.UnitTests.Application.Features.Commands.ResetPassword;

public sealed class ResetPasswordHandlerTests
{
    [Fact]
    public async Task Handle_WhenCommandIsInvalid_DoesNotAccessAccount()
    {
        var accounts = new Mock<IUserAccountService>();
        var handler = new ResetPasswordHandler(new ResetPasswordCommandValidator(), accounts.Object, Mock.Of<IRefreshSessionManager>(), Mock.Of<IUnitOfWork>(), NullLogger<ResetPasswordHandler>.Instance);

        var result = await handler.Handle(new ResetPasswordCommand("invalid", "", "Password1!", "Password1!"));

        Assert.True(result.IsFailure);
        accounts.Verify(item => item.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenResetSucceeds_ReturnsSuccess()
    {
        var user = new UserAccount(Guid.NewGuid(), "user@example.com", true);
        var accounts = new Mock<IUserAccountService>();
        accounts.Setup(item => item.FindByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        accounts.Setup(item => item.ResetPasswordAsync(user.Id, "code", "Password1!", It.IsAny<CancellationToken>())).ReturnsAsync(UnitResult.Success<Error>());
        var refreshSessions = new Mock<IRefreshSessionManager>();
        var transaction = new Mock<ITransaction>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(item => item.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transaction.Object);
        var handler = new ResetPasswordHandler(new ResetPasswordCommandValidator(), accounts.Object, refreshSessions.Object, unitOfWork.Object, NullLogger<ResetPasswordHandler>.Instance);

        var result = await handler.Handle(new ResetPasswordCommand("user@example.com", "code", "Password1!", "Password1!"));

        Assert.True(result.IsSuccess);
        accounts.Verify(item => item.ResetPasswordAsync(user.Id, "code", "Password1!", It.IsAny<CancellationToken>()), Times.Once);
        refreshSessions.Verify(item => item.RevokeAllForUserAsync(user.Id, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
        transaction.Verify(item => item.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenResetCodeIsInvalid_DoesNotRevokeSessions()
    {
        var user = new UserAccount(Guid.NewGuid(), "user@example.com", true);
        var accounts = new Mock<IUserAccountService>();
        accounts.Setup(item => item.FindByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        accounts.Setup(item => item.ResetPasswordAsync(user.Id, "wrong-code", "Password1!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(UnitResult.Failure(AuthErrors.TokenInvalid()));
        var refreshSessions = new Mock<IRefreshSessionManager>();
        var transaction = new Mock<ITransaction>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(item => item.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transaction.Object);
        var handler = new ResetPasswordHandler(new ResetPasswordCommandValidator(), accounts.Object, refreshSessions.Object, unitOfWork.Object, NullLogger<ResetPasswordHandler>.Instance);

        var result = await handler.Handle(new ResetPasswordCommand("user@example.com", "wrong-code", "Password1!", "Password1!"));

        Assert.True(result.IsFailure);
        refreshSessions.Verify(item => item.RevokeAllForUserAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
        transaction.Verify(item => item.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
