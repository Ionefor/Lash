using Lash.Users.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lash.Users.UnitTests.Infrastructure.Identity;

public sealed class EmailCodeTokenProviderTests
{
    [Fact]
    public async Task GenerateAsync_WhenEmailIsNotConfirmed_ReturnsSixDigitCode()
    {
        var user = IdentityUserEntity.Create("user@example.com");
        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.GetEmailAsync(user)).ReturnsAsync(user.Email);
        userManager.Setup(manager => manager.CreateSecurityTokenAsync(user))
            .ReturnsAsync([1, 2, 3, 4, 5, 6, 7, 8]);
        var provider = new EmailCodeTokenProvider<IdentityUserEntity>();

        var code = await provider.GenerateAsync("EmailConfirmation", userManager.Object, user);

        Assert.Matches("^\\d{6}$", code);
        Assert.False(user.EmailConfirmed);
    }

    [Fact]
    public async Task CanGenerateTwoFactorTokenAsync_WhenUserHasEmailButItIsNotConfirmed_ReturnsTrue()
    {
        var user = IdentityUserEntity.Create("user@example.com");
        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.GetEmailAsync(user)).ReturnsAsync(user.Email);
        var provider = new EmailCodeTokenProvider<IdentityUserEntity>();

        var canGenerate = await provider.CanGenerateTwoFactorTokenAsync(userManager.Object, user);

        Assert.True(canGenerate);
    }

    private static Mock<UserManager<IdentityUserEntity>> CreateUserManager() =>
        new(
            Mock.Of<IUserStore<IdentityUserEntity>>(),
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            Mock.Of<ILogger<UserManager<IdentityUserEntity>>>());
}
