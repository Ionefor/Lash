using Microsoft.Extensions.Logging;

namespace Lash.Users.Infrastructure.Seeding;

public sealed class DatabaseSeeder(
    ISeederRegistry seederRegistry,
    ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var seeder in seederRegistry.All.OrderBy(seeder => seeder.Order))
        {
            logger.LogInformation("Running Users database seeder {SeederName}.", seeder.GetType().Name);
            await seeder.SeedAsync(cancellationToken);
            logger.LogInformation("Users database seeder {SeederName} completed.", seeder.GetType().Name);
        }
    }
}
