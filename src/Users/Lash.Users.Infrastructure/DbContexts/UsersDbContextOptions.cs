using Microsoft.EntityFrameworkCore;

namespace Lash.Users.Infrastructure.DbContexts;

internal static class UsersDbContextOptions
{
    internal static void Configure(DbContextOptionsBuilder options, string connectionString)
    {
        options
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable(
                "__EFMigrationsHistory",
                UsersDbContext.SchemaName))
            .UseSnakeCaseNamingConvention();
    }
}
