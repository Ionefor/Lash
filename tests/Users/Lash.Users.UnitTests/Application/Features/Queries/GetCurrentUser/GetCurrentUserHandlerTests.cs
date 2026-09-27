using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Features.Queries.GetCurrentUser;
using Lash.Users.Application.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Lash.Users.UnitTests.Application.Features.Queries.GetCurrentUser;

public sealed class GetCurrentUserHandlerTests
{
    [Fact]
    public async Task Handle_WhenQueryIsInvalid_DoesNotAccessAccount()
    {
        var accounts = new Mock<IUserAccountService>();
        var handler = new GetCurrentUserHandler(new GetCurrentUserQueryValidator(), accounts.Object, NullLogger<GetCurrentUserHandler>.Instance);

        var result = await handler.Handle(new GetCurrentUserQuery(Guid.Empty));

        Assert.True(result.IsFailure);
        accounts.Verify(item => item.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ReturnsProfile()
    {
        var user = new UserAccount(Guid.NewGuid(), "user@example.com", true);
        var accounts = new Mock<IUserAccountService>();
        accounts.Setup(item => item.FindByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        accounts.Setup(item => item.GetRolesAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(["client"]);
        var handler = new GetCurrentUserHandler(new GetCurrentUserQueryValidator(), accounts.Object, NullLogger<GetCurrentUserHandler>.Instance);

        var result = await handler.Handle(new GetCurrentUserQuery(user.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Value.Id);
        Assert.Equal("user@example.com", result.Value.Email);
        Assert.Equal(["client"], result.Value.Roles);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsNotFoundForUserId()
    {
        var userId = Guid.NewGuid();
        var accounts = new Mock<IUserAccountService>();
        accounts.Setup(item => item.FindByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((UserAccount?)null);
        var handler = new GetCurrentUserHandler(new GetCurrentUserQueryValidator(), accounts.Object, NullLogger<GetCurrentUserHandler>.Instance);

        var result = await handler.Handle(new GetCurrentUserQuery(userId));

        Assert.True(result.IsFailure);
        Assert.Equal(GeneralErrorCodes.NotFound, result.Error[0].Code);
        Assert.Equal(ErrorType.NotFound, result.Error[0].Type);
        Assert.Equal(nameof(GetCurrentUserQuery.UserId), result.Error[0].Target);
        accounts.Verify(item => item.GetRolesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
