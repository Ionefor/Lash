using Lash.Users.Presentation.Controllers;
using Lash.Users.Presentation.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Lash.Users.UnitTests.Presentation.Controllers;

public sealed class RateLimitAttributesTests
{
    [Theory]
    [InlineData(typeof(AuthController), nameof(AuthController.Login), UsersRateLimitPolicies.Login)]
    [InlineData(typeof(AuthController), nameof(AuthController.Refresh), UsersRateLimitPolicies.Refresh)]
    [InlineData(typeof(AuthController), nameof(AuthController.Logout), UsersRateLimitPolicies.Refresh)]
    [InlineData(typeof(RegistrationController), nameof(RegistrationController.RegisterClient), UsersRateLimitPolicies.Registration)]
    [InlineData(typeof(RegistrationController), nameof(RegistrationController.RegisterMaster), UsersRateLimitPolicies.Registration)]
    public void PublicSensitiveAction_HasExpectedRateLimitPolicy(Type controllerType, string actionName, string expectedPolicy)
    {
        var action = controllerType.GetMethod(actionName);

        Assert.NotNull(action);
        var attribute = Assert.Single(action.GetCustomAttributes(inherit: false).OfType<EnableRateLimitingAttribute>());
        Assert.Equal(expectedPolicy, attribute.PolicyName);
    }
}
