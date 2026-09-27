using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Features.Commands.RequestPasswordReset;
using Lash.Users.Application.Models;
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
            Mock.Of<IPasswordResetSender>(),
            limiter.Object,
            Mock.Of<IUnitOfWork>(),
            NullLogger<RequestPasswordResetHandler>.Instance);

        var result = await handler.Handle(new RequestPasswordResetCommand("invalid"));

        Assert.True(result.IsFailure);
        limiter.Verify(item => item.TryAcquireAsync(It.IsAny<string>(), It.IsAny<IdentityEmailOperation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAccountExists_SendsPasswordResetAndReturnsSuccess()
    {
        var user = new UserAccount(Guid.NewGuid(), "user@example.com", true);
        var accounts = new Mock<IUserAccountService>();
        accounts.Setup(item => item.FindByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var sender = new Mock<IPasswordResetSender>();
        sender.Setup(item => item.SendAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(UnitResult.Success<Error>());
        var limiter = new Mock<IIdentityEmailRequestLimiter>();
        limiter.Setup(item => item.TryAcquireAsync("user@example.com", IdentityEmailOperation.PasswordReset, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var handler = new RequestPasswordResetHandler(new RequestPasswordResetCommandValidator(), accounts.Object, sender.Object, limiter.Object, unitOfWork.Object, NullLogger<RequestPasswordResetHandler>.Instance);

        var result = await handler.Handle(new RequestPasswordResetCommand("user@example.com"));

        Assert.True(result.IsSuccess);
        sender.Verify(item => item.SendAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
