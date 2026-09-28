using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Features.Commands.ConfirmEmail;
using Lash.Users.Application.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Lash.Users.UnitTests.Application.Features.Commands.ConfirmEmail;

public sealed class ConfirmEmailHandlerTests
{
    [Fact]
    public async Task Handle_WhenCommandIsInvalid_DoesNotAccessAccount()
    {
        var accounts = new Mock<IUserAccountService>();
        var handler = new ConfirmEmailHandler(new ConfirmEmailCommandValidator(), accounts.Object, Mock.Of<IIdentityEmailCodeAttemptLimiter>(), NullLogger<ConfirmEmailHandler>.Instance);

        var result = await handler.Handle(new ConfirmEmailCommand("", ""));

        Assert.True(result.IsFailure);
        accounts.Verify(item => item.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenConfirmationSucceeds_ReturnsSuccess()
    {
        var user = new UserAccount(Guid.NewGuid(), "user@example.com", false);
        var accounts = new Mock<IUserAccountService>();
        accounts.Setup(item => item.FindByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        accounts.Setup(item => item.ConfirmEmailAsync(user.Id, "code", It.IsAny<CancellationToken>())).ReturnsAsync(UnitResult.Success<Error>());
        var attempts = new Mock<IIdentityEmailCodeAttemptLimiter>();
        attempts.Setup(item => item.TryAcquireAsync("user@example.com", IdentityEmailOperation.EmailConfirmation, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new ConfirmEmailHandler(new ConfirmEmailCommandValidator(), accounts.Object, attempts.Object, NullLogger<ConfirmEmailHandler>.Instance);

        var result = await handler.Handle(new ConfirmEmailCommand("user@example.com", "code"));

        Assert.True(result.IsSuccess);
        accounts.Verify(item => item.ConfirmEmailAsync(user.Id, "code", It.IsAny<CancellationToken>()), Times.Once);
        attempts.Verify(item => item.CompleteAsync("user@example.com", IdentityEmailOperation.EmailConfirmation, It.IsAny<CancellationToken>()), Times.Once);
    }
}
