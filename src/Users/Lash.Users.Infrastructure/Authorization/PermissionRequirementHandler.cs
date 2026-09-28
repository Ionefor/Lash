using Lash.Users.Application.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Lash.Users.Infrastructure.Authorization;

public sealed class PermissionRequirementHandler(IServiceScopeFactory scopeFactory)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdValue = context.User.FindFirst(AccessTokenClaimTypes.Sub)?.Value;
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            context.Fail();
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var permissionManager = scope.ServiceProvider.GetRequiredService<IPermissionManager>();
        var permissions = await permissionManager.GetUserPermissionsAsync(userId);
        if (permissions.Contains(requirement.Code))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
