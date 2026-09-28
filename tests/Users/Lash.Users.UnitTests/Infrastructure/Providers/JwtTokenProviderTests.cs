using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ErrorsFlow.Errors;
using Lash.Users.Application.Constants;
using Lash.Users.Application.Models;
using Lash.Users.Infrastructure.Authorization;
using Lash.Users.Infrastructure.DbContexts;
using Lash.Users.Infrastructure.Identity;
using Lash.Users.Infrastructure.Options;
using Lash.Users.Infrastructure.Providers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Lash.Users.UnitTests.Infrastructure.Providers;

public sealed class JwtTokenProviderTests
{
    [Fact]
    public async Task GenerateAccessTokenAsync_WhenUserHasRolesAndPermissions_IncludesExpectedClaims()
    {
        var user = new UserAccount(Guid.NewGuid(), "user@example.com", true);
        var identityUser = IdentityUserEntity.Create(user.Email!);
        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByIdAsync(user.Id.ToString())).ReturnsAsync(identityUser);
        userManager.Setup(manager => manager.GetRolesAsync(identityUser)).ReturnsAsync(["Client"]);
        var permissions = new Mock<IPermissionManager>();
        permissions.Setup(manager => manager.GetUserPermissionsAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>(["users.read"]));
        var provider = CreateProvider(userManager.Object, permissions.Object);

        var token = await provider.GenerateAccessTokenAsync(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);

        Assert.Equal(token.Jti.ToString(), jwt.Claims.Single(claim => claim.Type == AccessTokenClaimTypes.Jti).Value);
        Assert.Equal(user.Id.ToString(), jwt.Claims.Single(claim => claim.Type == AccessTokenClaimTypes.Sub).Value);
        Assert.Equal(user.Email, jwt.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Contains(jwt.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == "Client");
        Assert.Contains(jwt.Claims, claim => claim.Type == CustomClaims.Permission && claim.Value == "users.read");
    }

    [Fact]
    public async Task GetClaimsFromExpiredAccessTokenAsync_WhenTokenIsInvalid_ReturnsTokenInvalidError()
    {
        var provider = CreateProvider(CreateUserManager().Object, Mock.Of<IPermissionManager>());

        var result = await provider.GetClaimsFromExpiredAccessTokenAsync("not-a-jwt");

        Assert.True(result.IsFailure);
        Assert.Equal(AuthErrorCodes.TokenInvalid, result.Error.Code);
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_WhenAbsoluteExpiryIsInPast_ReturnsValidationErrorWithoutWritingSession()
    {
        await using var context = CreateContext();
        var provider = new JwtTokenProvider(
            Options.Create(CreateOptions()),
            context,
            CreateUserManager().Object,
            Mock.Of<IPermissionManager>());

        var result = await provider.GenerateRefreshTokenAsync(
            new UserAccount(Guid.NewGuid(), "user@example.com", true),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddMinutes(-1));

        Assert.True(result.IsFailure);
        Assert.Equal(GeneralErrorCodes.ValueIsInvalid, result.Error.Code);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    private static JwtTokenProvider CreateProvider(
        UserManager<IdentityUserEntity> userManager,
        IPermissionManager permissionManager) =>
        new(
            Options.Create(CreateOptions()),
            CreateContext(),
            userManager,
            permissionManager);

    private static JwtOptions CreateOptions() => new()
    {
        Issuer = "lash",
        Audience = "lash-client",
        Key = "a-secure-signing-key-with-at-least-32-characters",
        ExpiredMinutesTime = 15,
        RefreshTokenLifetimeDays = 30,
        RefreshTokenAbsoluteLifetimeDays = 90
    };

    private static UsersDbContext CreateContext() => new(
        new DbContextOptionsBuilder<UsersDbContext>()
            .UseNpgsql("Host=localhost;Database=lash_users;Username=test;Password=test")
            .Options);

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
