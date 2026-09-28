using Lash.Users.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lash.Users.Infrastructure.Extensions;

internal static class UsersDatabaseServiceCollectionExtensions
{
    internal static IServiceCollection AddUsersDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("UsersDatabase");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'UsersDatabase' is not configured.");
        }

        services.AddDbContext<UsersDbContext>(options =>
            UsersDbContextOptions.Configure(options, connectionString));

        return services;
    }
}
