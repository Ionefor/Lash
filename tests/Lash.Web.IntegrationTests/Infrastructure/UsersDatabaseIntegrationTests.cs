using Lash.Users.Application.Abstractions;
using Lash.Users.Domain;
using Lash.Users.Infrastructure.DbContexts;
using Lash.Users.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lash.Web.IntegrationTests.Infrastructure;

public sealed class UsersDatabaseIntegrationTests
{
    [DockerFact]
    public async Task Host_WhenStarted_AppliesUsersMigrationAndAtomicallyRevokesRefreshSessionInPostgreSql()
    {
        var factory = new ContainerizedLashWebApplicationFactory();
        try
        {
            await factory.StartAsync();
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("/health/live");
            Assert.True(response.IsSuccessStatusCode);

            using var readinessResponse = await client.GetAsync("/health/ready");
            Assert.True(readinessResponse.IsSuccessStatusCode);

            await using var scope = factory.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
            var migrations = await dbContext.Database.GetAppliedMigrationsAsync();

            Assert.Contains("20260928054029_InitialUsers", migrations);
            Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", dbContext.Database.ProviderName);
            Assert.Equal(0, await dbContext.Users.CountAsync());

            var user = IdentityUserEntity.Create($"user-{Guid.NewGuid():N}@example.com");
            var now = DateTimeOffset.UtcNow;
            var sessionResult = RefreshSession.Create(
                user.Id,
                Guid.NewGuid(),
                "test-token-hash",
                now,
                now.AddDays(1),
                now.AddDays(7));
            Assert.True(sessionResult.IsSuccess);

            dbContext.Users.Add(user);
            dbContext.RefreshSessions.Add(sessionResult.Value);
            await dbContext.SaveChangesAsync();

            var revocations = await Task.WhenAll(
                TryRevokeAsync(factory, sessionResult.Value.Id, now),
                TryRevokeAsync(factory, sessionResult.Value.Id, now));

            Assert.Equal(1, revocations.Count(revoked => revoked));
        }
        finally
        {
            await factory.StopAsync();
        }
    }

    private static async Task<bool> TryRevokeAsync(
        ContainerizedLashWebApplicationFactory factory,
        Guid sessionId,
        DateTimeOffset revokedAt)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var sessions = scope.ServiceProvider.GetRequiredService<IRefreshSessionManager>();
        return await sessions.TryRevokeAsync(sessionId, revokedAt);
    }
}
