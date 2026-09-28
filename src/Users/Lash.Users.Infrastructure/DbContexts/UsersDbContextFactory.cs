using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Lash.Users.Infrastructure.DbContexts;

public sealed class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__UsersDatabase");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings__UsersDatabase must be configured to run EF Core tooling.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();
        UsersDbContextOptions.Configure(optionsBuilder, connectionString);

        return new UsersDbContext(optionsBuilder.Options);
    }
}
