using Lash.Users.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lash.Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddUsersDatabase(configuration);
        services.AddUsersIdentity();
        services.AddUsersOptions(configuration);
        services.AddUsersCoreServices();
        services.AddUsersMessaging();
        services.AddUsersAuthentication();

        return services;
    }
}
