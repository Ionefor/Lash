using System.Security.Claims;
using Lash.Users.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Lash.Users.UnitTests.Infrastructure.Authorization;

public sealed class PermissionRequirementHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserHasPermissionClaim_SucceedsRequirement()
    {
        const string permissionCode = "users.read";
        var requirement = new PermissionRequirement(permissionCode);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(CustomClaims.Permission, permissionCode)],
            "Test"));
        var context = new AuthorizationHandlerContext([requirement], principal, null);
        var handler = new PermissionRequirementHandler();

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotHavePermissionClaim_LeavesRequirementPending()
    {
        var requirement = new PermissionRequirement("users.read");
        var principal = new ClaimsPrincipal(new ClaimsIdentity(authenticationType: "Test"));
        var context = new AuthorizationHandlerContext([requirement], principal, null);
        var handler = new PermissionRequirementHandler();

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }
}
