using Lash.Users.Infrastructure.DbContexts;
using Lash.Users.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lash.Users.Infrastructure.Extensions;

public static class DatabaseInitializationExtensions
{
    public static async Task MigrateUsersDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;
        var logger = serviceProvider.GetRequiredService<ILogger<UsersDbContext>>();

        try
        {
            logger.LogInformation("Applying Users database migrations.");
            var dbContext = serviceProvider.GetRequiredService<UsersDbContext>();
            await dbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Users database migrations applied.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Users database migration failed.");
            throw;
        }
    }

    public static async Task SeedUsersDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;
        var logger = serviceProvider.GetRequiredService<ILogger<DatabaseSeeder>>();

        try
        {
            logger.LogInformation("Starting Users database seed.");
            var databaseSeeder = serviceProvider.GetRequiredService<DatabaseSeeder>();
            await databaseSeeder.SeedAsync(cancellationToken);
            logger.LogInformation("Users database seed completed.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Users database seed failed.");
            throw;
        }
    }
}
