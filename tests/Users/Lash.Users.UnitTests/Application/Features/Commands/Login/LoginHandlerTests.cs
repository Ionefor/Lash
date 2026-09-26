using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Features.Commands.Login;
using Lash.Users.Application.Models;
using Lash.Users.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Lash.Users.UnitTests.Application.Features.Commands.Login;

public sealed class LoginHandlerTests
{
    [Fact]
    public async Task Handle_WhenPasswordIsInvalid_IncrementsAccessFailureCount()
    {
        var user = CreateUser();
        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        userManager.Setup(manager => manager.IsLockedOutAsync(user)).ReturnsAsync(false);
        userManager.Setup(manager => manager.CheckPasswordAsync(user, "Incorrect1!")).ReturnsAsync(false);
        userManager.Setup(manager => manager.AccessFailedAsync(user)).ReturnsAsync(IdentityResult.Success);

        var handler = new LoginHandler(new LoginCommandValidator(), userManager.Object, Mock.Of<ITokenProvider>());

        var result = await handler.Handle(new LoginCommand(user.Email!, "Incorrect1!"));

        Assert.True(result.IsFailure);
        Assert.Equal(AuthErrorCodes.CredentialsInvalid, result.Error[0].Code);
        userManager.Verify(manager => manager.AccessFailedAsync(user), Times.Once);
        userManager.Verify(manager => manager.ResetAccessFailedCountAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIsLockedOut_ReturnsCredentialsInvalidWithoutCheckingPassword()
    {
        var user = CreateUser();
        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        userManager.Setup(manager => manager.IsLockedOutAsync(user)).ReturnsAsync(true);

        var handler = new LoginHandler(new LoginCommandValidator(), userManager.Object, Mock.Of<ITokenProvider>());

        var result = await handler.Handle(new LoginCommand(user.Email!, "Password1!"));

        Assert.True(result.IsFailure);
        Assert.Equal(AuthErrorCodes.CredentialsInvalid, result.Error[0].Code);
        userManager.Verify(manager => manager.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        userManager.Verify(manager => manager.AccessFailedAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCredentialsAreValid_ResetsAccessFailureCountAndReturnsTokens()
    {
        var user = CreateUser();
        var tokenJti = Guid.NewGuid();
        var userManager = CreateUserManager();
        var tokenProvider = new Mock<ITokenProvider>();
        userManager.Setup(manager => manager.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        userManager.Setup(manager => manager.IsLockedOutAsync(user)).ReturnsAsync(false);
        userManager.Setup(manager => manager.CheckPasswordAsync(user, "Password1!")).ReturnsAsync(true);
        userManager.Setup(manager => manager.ResetAccessFailedCountAsync(user)).ReturnsAsync(IdentityResult.Success);
        tokenProvider.Setup(provider => provider.GenerateAccessTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JwtTokenResult("access", tokenJti));
        tokenProvider.Setup(provider => provider.GenerateRefreshTokenAsync(user, tokenJti, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<string, Error>("refresh"));

        var handler = new LoginHandler(new LoginCommandValidator(), userManager.Object, tokenProvider.Object);

        var result = await handler.Handle(new LoginCommand(user.Email!, "Password1!"));

        Assert.True(result.IsSuccess);
        Assert.Equal(new AuthTokens("access", "refresh"), result.Value);
        userManager.Verify(manager => manager.ResetAccessFailedCountAsync(user), Times.Once);
    }

    private static Mock<UserManager<User>> CreateUserManager()
    {
        return new Mock<UserManager<User>>(
            new Mock<IUserStore<User>>().Object,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<User>(),
            Array.Empty<IUserValidator<User>>(),
            Array.Empty<IPasswordValidator<User>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            NullLogger<UserManager<User>>.Instance);
    }

    private static User CreateUser()
    {
        var role = Role.Create(RoleNames.Client).Value;
        var user = User.RegisterClient("user@example.com", role).Value;
        user.EmailConfirmed = true;
        return user;
    }
}
