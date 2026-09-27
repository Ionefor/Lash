using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Features.Commands.ResetPassword;
using Lash.Users.Application.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Lash.Users.UnitTests.Application.Features.Commands.ResetPassword;

public sealed class ResetPasswordHandlerTests
{
    [Fact]
    public async Task Handle_WhenCommandIsInvalid_DoesNotAccessAccount()
    {
        var accounts = new Mock<IUserAccountService>();
        var handler = new ResetPasswordHandler(new ResetPasswordCommandValidator(), accounts.Object, NullLogger<ResetPasswordHandler>.Instance);

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
        var handler = new ResetPasswordHandler(new ResetPasswordCommandValidator(), accounts.Object, NullLogger<ResetPasswordHandler>.Instance);

        var result = await handler.Handle(new ResetPasswordCommand("user@example.com", "code", "Password1!", "Password1!"));

        Assert.True(result.IsSuccess);
        accounts.Verify(item => item.ResetPasswordAsync(user.Id, "code", "Password1!", It.IsAny<CancellationToken>()), Times.Once);
    }
}
