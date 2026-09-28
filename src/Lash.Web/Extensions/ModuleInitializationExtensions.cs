using Lash.Users.Infrastructure.Extensions;
using Lash.Users.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Lash.Web.Extensions;

public static class ModuleInitializationExtensions
{
    public static async Task InitializeLashModulesAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        var databaseInitialization = services
            .GetRequiredService<IOptions<DatabaseInitializationOptions>>().Value;

        if (databaseInitialization.ApplyMigrationsOnStartup)
        {
            await services.MigrateUsersDatabaseAsync(cancellationToken);
        }

        if (databaseInitialization.ApplySeedOnStartup)
        {
            await services.SeedUsersDatabaseAsync(cancellationToken);
        }
    }
}
