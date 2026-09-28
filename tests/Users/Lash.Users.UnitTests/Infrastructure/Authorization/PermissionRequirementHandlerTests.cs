using System.Security.Claims;
using Lash.Users.Application.Constants;
using Lash.Users.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Lash.Users.UnitTests.Infrastructure.Authorization;

public sealed class PermissionRequirementHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserHasCurrentPermission_SucceedsRequirement()
    {
        const string permissionCode = "users.read";
        var userId = Guid.NewGuid();
        var permissions = new Mock<IPermissionManager>();
        permissions.Setup(manager => manager.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>([permissionCode]));
        var requirement = new PermissionRequirement(permissionCode);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(AccessTokenClaimTypes.Sub, userId.ToString())],
            "Test"));
        var context = new AuthorizationHandlerContext([requirement], principal, null);
        await using var serviceProvider = CreateServiceProvider(permissions.Object);
        var handler = new PermissionRequirementHandler(
            serviceProvider.GetRequiredService<IServiceScopeFactory>());

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
        permissions.Verify(manager => manager.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotHaveCurrentPermission_FailsRequirement()
    {
        var userId = Guid.NewGuid();
        var permissions = new Mock<IPermissionManager>();
        permissions.Setup(manager => manager.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());
        var requirement = new PermissionRequirement("users.read");
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(AccessTokenClaimTypes.Sub, userId.ToString())],
            "Test"));
        var context = new AuthorizationHandlerContext([requirement], principal, null);
        await using var serviceProvider = CreateServiceProvider(permissions.Object);
        var handler = new PermissionRequirementHandler(
            serviceProvider.GetRequiredService<IServiceScopeFactory>());

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
    }

    private static ServiceProvider CreateServiceProvider(IPermissionManager permissionManager) =>
        new ServiceCollection()
            .AddScoped(_ => permissionManager)
            .BuildServiceProvider();
}
