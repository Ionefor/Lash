using Microsoft.AspNetCore.Authorization;

namespace Lash.Users.Infrastructure.Authorization;

public sealed class PermissionRequirementHandler
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.HasClaim(CustomClaims.Permission, requirement.Code))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
