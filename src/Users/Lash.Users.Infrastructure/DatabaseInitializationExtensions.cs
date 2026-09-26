using Lash.Users.Infrastructure.DbContexts;
using Lash.Users.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lash.Users.Infrastructure;

public static class DatabaseInitializationExtensions
{
    public static async Task MigrateUsersDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;

        var dbContext = serviceProvider.GetRequiredService<UsersDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);

        var rolesSeeder = serviceProvider.GetRequiredService<RolesSeeder>();
        await rolesSeeder.SeedAsync(cancellationToken);

        var permissionsSeeder = serviceProvider.GetRequiredService<PermissionsSeeder>();
        await permissionsSeeder.SeedAsync(cancellationToken);

        var adminSeeder = serviceProvider.GetRequiredService<AdminSeeder>();
        await adminSeeder.SeedAsync(cancellationToken);
    }
}
