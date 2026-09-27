using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace Lash.Users.Presentation;

public static class DependencyInjection
{
    public static IMvcBuilder AddUsersPresentation(this IMvcBuilder builder)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        if (builder.PartManager.ApplicationParts
            .OfType<AssemblyPart>()
            .Any(part => part.Assembly == assembly))
        {
            return builder;
        }

        return builder.AddApplicationPart(assembly);
    }
}
