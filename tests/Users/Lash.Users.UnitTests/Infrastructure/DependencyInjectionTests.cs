using Lash.Users.Application.Abstractions;
using Lash.Users.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.UnitTests.Infrastructure;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddUsersInfrastructure_WhenConfigurationIsValid_ResolvesInfrastructureServices()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:UsersDatabase"] = "Host=localhost;Database=lash;Username=lash;Password=lash",
                ["Jwt:Issuer"] = "lash",
                ["Jwt:Audience"] = "lash-client",
                ["Jwt:Key"] = "a-secure-signing-key-with-at-least-32-characters",
                ["Jwt:ExpiredMinutesTime"] = "15",
                ["Jwt:RefreshTokenLifetimeDays"] = "30"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddUsersInfrastructure(configuration);
        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var tokenProvider = scope.ServiceProvider.GetRequiredService<ITokenProvider>();
        var permissionManager = scope.ServiceProvider.GetRequiredService<IPermissionManager>();
        var refreshSessionManager = scope.ServiceProvider.GetRequiredService<IRefreshSessionManager>();
        var identityEmailRequestLimiter = scope.ServiceProvider.GetRequiredService<IIdentityEmailRequestLimiter>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var authorizationOptions = serviceProvider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        Assert.NotNull(tokenProvider);
        Assert.NotNull(permissionManager);
        Assert.NotNull(refreshSessionManager);
        Assert.NotNull(identityEmailRequestLimiter);
        Assert.NotNull(unitOfWork);
        Assert.NotNull(authorizationOptions.FallbackPolicy);
        Assert.Contains(authorizationOptions.FallbackPolicy.Requirements, requirement => requirement is DenyAnonymousAuthorizationRequirement);
        var identityOptions = serviceProvider.GetRequiredService<IOptions<IdentityOptions>>().Value;
        Assert.True(identityOptions.Lockout.AllowedForNewUsers);
        Assert.Equal(5, identityOptions.Lockout.MaxFailedAccessAttempts);
        Assert.Equal(TimeSpan.FromMinutes(15), identityOptions.Lockout.DefaultLockoutTimeSpan);
    }
}
