using Lash.Users.Presentation;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace Lash.Users.UnitTests.Presentation.DependencyInjection;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddUsersPresentation_WhenCalledMoreThanOnce_AddsApplicationPartOnce()
    {
        var services = new ServiceCollection();
        var mvcBuilder = services.AddControllers();

        mvcBuilder.AddUsersPresentation();
        mvcBuilder.AddUsersPresentation();

        var parts = mvcBuilder.PartManager.ApplicationParts
            .OfType<AssemblyPart>()
            .Where(part => part.Assembly == typeof(Lash.Users.Presentation.DependencyInjection).Assembly);

        Assert.Single(parts);
    }
}
