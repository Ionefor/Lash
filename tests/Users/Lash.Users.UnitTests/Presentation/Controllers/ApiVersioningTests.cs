using Asp.Versioning;
using Lash.Users.Presentation.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Lash.Users.UnitTests.Presentation.Controllers;

public sealed class ApiVersioningTests
{
    [Theory]
    [InlineData(typeof(AuthController), "api/v{version:apiVersion}/auth")]
    [InlineData(typeof(RegistrationController), "api/v{version:apiVersion}/registration")]
    public void Controller_UsesUrlSegmentVersionOne(Type controllerType, string expectedRoute)
    {
        var route = Assert.Single(
            controllerType.GetCustomAttributes(inherit: false).OfType<RouteAttribute>(),
            attribute => attribute.GetType() == typeof(RouteAttribute));
        var version = Assert.IsType<ApiVersionAttribute>(Attribute.GetCustomAttribute(controllerType, typeof(ApiVersionAttribute)));
        var versionProvider = Assert.IsAssignableFrom<IApiVersionProvider>(version);

        Assert.Equal(expectedRoute, route.Template);
        Assert.Contains(new ApiVersion(1, 0), versionProvider.Versions);
    }
}
