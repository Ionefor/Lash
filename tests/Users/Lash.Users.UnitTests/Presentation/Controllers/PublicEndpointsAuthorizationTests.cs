using Lash.Users.Presentation.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace Lash.Users.UnitTests.Presentation.Controllers;

public sealed class PublicEndpointsAuthorizationTests
{
    [Theory]
    [InlineData(typeof(AuthController), nameof(AuthController.Login))]
    [InlineData(typeof(AuthController), nameof(AuthController.Refresh))]
    [InlineData(typeof(AuthController), nameof(AuthController.ConfirmEmail))]
    [InlineData(typeof(AuthController), nameof(AuthController.ResendEmailConfirmation))]
    [InlineData(typeof(AuthController), nameof(AuthController.RequestPasswordReset))]
    [InlineData(typeof(AuthController), nameof(AuthController.ResetPassword))]
    [InlineData(typeof(AuthController), nameof(AuthController.Logout))]
    [InlineData(typeof(RegistrationController), nameof(RegistrationController.RegisterClient))]
    [InlineData(typeof(RegistrationController), nameof(RegistrationController.RegisterMaster))]
    public void PublicAction_HasAllowAnonymousAttribute(Type controllerType, string actionName)
    {
        var action = controllerType.GetMethod(actionName);

        Assert.NotNull(action);
        Assert.Contains(action.GetCustomAttributes(inherit: false), attribute => attribute is AllowAnonymousAttribute);
    }
}
