using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Features.Commands.ChangePassword;
using Lash.Users.Application.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Lash.Users.UnitTests.Application.Features.Commands.ChangePassword;

public sealed class ChangePasswordHandlerTests
{
    [Fact]
    public async Task Handle_WhenCommandIsInvalid_DoesNotAccessAccount()
    {
        var accounts = new Mock<IUserAccountService>();
        var handler = new ChangePasswordHandler(new ChangePasswordCommandValidator(), accounts.Object, NullLogger<ChangePasswordHandler>.Instance);

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
        var handler = new ChangePasswordHandler(new ChangePasswordCommandValidator(), accounts.Object, NullLogger<ChangePasswordHandler>.Instance);

        var result = await handler.Handle(new ChangePasswordCommand(user.Id, "Current1!", "Password1!", "Password1!"));

        Assert.True(result.IsSuccess);
        accounts.Verify(item => item.ChangePasswordAsync(user.Id, "Current1!", "Password1!", It.IsAny<CancellationToken>()), Times.Once);
    }
}
