using Lash.Users.Infrastructure.DbContexts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Lash.Web.HealthChecks;

public sealed class UsersDatabaseHealthCheck(IServiceScopeFactory scopeFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Users database is unavailable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Users database is unavailable.", exception);
        }
    }
}
