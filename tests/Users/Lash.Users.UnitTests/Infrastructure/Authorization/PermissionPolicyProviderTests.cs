using Lash.Users.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;

namespace Lash.Users.UnitTests.Infrastructure.Authorization;

public sealed class PermissionPolicyProviderTests
{
    [Fact]
    public async Task GetPolicyAsync_WhenPolicyIsNotConfigured_CreatesPermissionPolicy()
    {
        const string permissionCode = "users.read";
        var provider = new PermissionPolicyProvider(Options.Create(new AuthorizationOptions()));

        var policy = await provider.GetPolicyAsync(permissionCode);

        Assert.NotNull(policy);
        var requirement = Assert.Single(policy.Requirements.OfType<PermissionRequirement>());
        Assert.Equal(permissionCode, requirement.Code);
        Assert.Contains(policy.Requirements, item => item is DenyAnonymousAuthorizationRequirement);
    }
}
