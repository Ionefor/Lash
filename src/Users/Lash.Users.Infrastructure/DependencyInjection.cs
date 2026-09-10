using Lash.Users.Domain;
using Lash.Users.Infrastructure.DbContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lash.Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("UsersDatabase")
            ?? throw new InvalidOperationException("Connection string 'UsersDatabase' is not configured.");

        services.AddDbContext<UsersDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());

        services.AddIdentityCore<User>(options => options.User.RequireUniqueEmail = true)
            .AddRoles<Role>()
            .AddEntityFrameworkStores<UsersDbContext>();

        return services;
    }
}
