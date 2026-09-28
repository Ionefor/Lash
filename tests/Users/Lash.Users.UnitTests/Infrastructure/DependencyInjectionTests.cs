using Lash.Users.Application.Abstractions;
using Lash.Users.Infrastructure;
using Lash.Users.Infrastructure.Authorization;
using Lash.Users.Infrastructure.Identity;
using Lash.Users.Infrastructure.Options;
using Lash.Users.Infrastructure.Seeding;
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
    public void AddUsersInfrastructure_WhenConnectionStringIsWhitespace_RejectsConfiguration()
    {
        var configuration = CreateConfiguration(
            new KeyValuePair<string, string?>("ConnectionStrings:UsersDatabase", " "));
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddUsersInfrastructure(configuration));
    }

    [Fact]
    public async Task AddUsersInfrastructure_WhenConfigurationIsValid_ResolvesInfrastructureServices()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:UsersDatabase"] = "Host=localhost;Database=lash;Username=lash;Password=lash",
                ["Jwt:Issuer"] = "lash",
                ["Jwt:Audience"] = "lash-client",
                ["Jwt:Key"] = "a-secure-signing-key-with-at-least-32-characters",
                ["Jwt:ExpiredMinutesTime"] = "15",
                ["Jwt:RefreshTokenLifetimeDays"] = "30",
                ["Jwt:RefreshTokenAbsoluteLifetimeDays"] = "60",
                ["Email:Host"] = "localhost",
                ["Email:FromAddress"] = "noreply@example.com",
                ["RabbitMq:Host"] = "localhost",
                ["RabbitMq:VirtualHost"] = "/",
                ["RabbitMq:UserName"] = "guest",
                ["RabbitMq:Password"] = "guest"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddUsersInfrastructure(configuration);
        await using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        await using var scope = serviceProvider.CreateAsyncScope();

        var tokenProvider = scope.ServiceProvider.GetRequiredService<ITokenProvider>();
        var permissionManager = scope.ServiceProvider.GetRequiredService<IPermissionManager>();
        var refreshSessionManager = scope.ServiceProvider.GetRequiredService<IRefreshSessionManager>();
        var identityEmailRequestLimiter = scope.ServiceProvider.GetRequiredService<IIdentityEmailRequestLimiter>();
        var registrationEventPublisher = scope.ServiceProvider.GetRequiredService<IUsersEventPublisher>();
        var databaseSeeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var authorizationOptions = serviceProvider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        Assert.NotNull(tokenProvider);
        Assert.NotNull(permissionManager);
        Assert.NotNull(refreshSessionManager);
        Assert.NotNull(identityEmailRequestLimiter);
        Assert.NotNull(registrationEventPublisher);
        Assert.NotNull(databaseSeeder);
        Assert.NotNull(unitOfWork);
        Assert.NotNull(authorizationOptions.FallbackPolicy);
        Assert.Contains(authorizationOptions.FallbackPolicy.Requirements, requirement => requirement is DenyAnonymousAuthorizationRequirement);
        var identityOptions = serviceProvider.GetRequiredService<IOptions<IdentityOptions>>().Value;
        Assert.True(identityOptions.Lockout.AllowedForNewUsers);
        Assert.Equal(5, identityOptions.Lockout.MaxFailedAccessAttempts);
        Assert.Equal(TimeSpan.FromMinutes(15), identityOptions.Lockout.DefaultLockoutTimeSpan);
        Assert.Equal(EmailCodeTokenProvider<IdentityUserEntity>.ProviderName, identityOptions.Tokens.EmailConfirmationTokenProvider);
        Assert.Equal(EmailCodeTokenProvider<IdentityUserEntity>.ProviderName, identityOptions.Tokens.PasswordResetTokenProvider);
    }

    [Fact]
    public void AddUsersInfrastructure_WhenSeedIsEnabledAndAdminCredentialsAreIncomplete_RejectsConfiguration()
    {
        var configuration = CreateConfiguration(
            new KeyValuePair<string, string?>("DatabaseInitialization:ApplySeedOnStartup", "true"),
            new KeyValuePair<string, string?>("Admin:Email", "admin@example.com"));
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddUsersInfrastructure(configuration);
        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        Assert.Throws<OptionsValidationException>(() =>
            serviceProvider.GetRequiredService<IOptions<AdminOptions>>().Value);
    }

    [Fact]
    public void AddUsersInfrastructure_WhenSeedIsEnabledAndRoleIsUnknown_RejectsConfiguration()
    {
        var configuration = CreateConfiguration(
            new KeyValuePair<string, string?>("DatabaseInitialization:ApplySeedOnStartup", "true"),
            new KeyValuePair<string, string?>("RolePermissions:Roles:Unknown:0", "appointments.read"));
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddUsersInfrastructure(configuration);
        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        Assert.Throws<OptionsValidationException>(() =>
            serviceProvider.GetRequiredService<IOptions<RolePermissionOptions>>().Value);
    }

    [Theory]
    [InlineData("RolePermissions:Permissions: :0", "appointments.read")]
    [InlineData("RolePermissions:Roles:Client:0", "")]
    [InlineData("RolePermissions:Roles:Client:0", "appointments.read", "RolePermissions:Roles:Client:1", "appointments.read")]
    public void AddUsersInfrastructure_WhenSeedPermissionsAreInvalid_RejectsConfiguration(
        string firstKey,
        string firstValue,
        string? secondKey = null,
        string? secondValue = null)
    {
        var values = new List<KeyValuePair<string, string?>>
        {
            new("DatabaseInitialization:ApplySeedOnStartup", "true"),
            new(firstKey, firstValue)
        };
        if (secondKey is not null)
        {
            values.Add(new KeyValuePair<string, string?>(secondKey, secondValue));
        }

        var configuration = CreateConfiguration(values.ToArray());
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddUsersInfrastructure(configuration);
        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        Assert.Throws<OptionsValidationException>(() =>
            serviceProvider.GetRequiredService<IOptions<RolePermissionOptions>>().Value);
    }

    [Fact]
    public void AddUsersInfrastructure_WhenSeedIsEnabledAndRoleHasNoPermissions_AcceptsConfiguration()
    {
        var configuration = CreateConfiguration(
            new KeyValuePair<string, string?>("DatabaseInitialization:ApplySeedOnStartup", "true"));
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddUsersInfrastructure(configuration);
        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        var options = serviceProvider.GetRequiredService<IOptions<RolePermissionOptions>>().Value;

        Assert.Empty(options.Roles);
    }

    [Fact]
    public void AddUsersInfrastructure_WhenSeedIsEnabledAndAdminCredentialsAreComplete_AcceptsConfiguration()
    {
        var configuration = CreateConfiguration(
            new KeyValuePair<string, string?>("DatabaseInitialization:ApplySeedOnStartup", "true"),
            new KeyValuePair<string, string?>("Admin:Email", "admin@example.com"),
            new KeyValuePair<string, string?>("Admin:Password", "Password1!"));
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddUsersInfrastructure(configuration);
        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        var options = serviceProvider.GetRequiredService<IOptions<AdminOptions>>().Value;

        Assert.Equal("admin@example.com", options.Email);
    }

    [Fact]
    public void AddUsersInfrastructure_WhenRabbitMqHostIsEmpty_RejectsConfiguration()
    {
        var configuration = CreateConfiguration(
            new KeyValuePair<string, string?>("RabbitMq:Host", ""));
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddUsersInfrastructure(configuration);
        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        Assert.Throws<OptionsValidationException>(() =>
            serviceProvider.GetRequiredService<IOptions<RabbitMqOptions>>().Value);
    }

    [Theory]
    [InlineData("Email:Port", "0")]
    [InlineData("Email:Port", "65536")]
    [InlineData("Email:FromAddress", "not-an-email-address")]
    public void AddUsersInfrastructure_WhenEmailSettingsAreInvalid_RejectsConfiguration(
        string key,
        string value)
    {
        var configuration = CreateConfiguration(new KeyValuePair<string, string?>(key, value));
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddUsersInfrastructure(configuration);
        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        Assert.Throws<OptionsValidationException>(() =>
            serviceProvider.GetRequiredService<IOptions<EmailOptions>>().Value);
    }

    [Fact]
    public void AddUsersInfrastructure_WhenIdentityEmailRateLimitIsNotPositive_RejectsConfiguration()
    {
        var configuration = CreateConfiguration(
            new KeyValuePair<string, string?>("IdentityEmailRateLimit:PasswordResetLimit", "0"));
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddUsersInfrastructure(configuration);
        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        Assert.Throws<OptionsValidationException>(() =>
            serviceProvider.GetRequiredService<IOptions<IdentityEmailRateLimitOptions>>().Value);
    }

    [Fact]
    public void AddUsersInfrastructure_WhenJwtKeyIsWhitespace_RejectsConfiguration()
    {
        var configuration = CreateConfiguration(
            new KeyValuePair<string, string?>("Jwt:Key", new string(' ', 32)));
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddUsersInfrastructure(configuration);
        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        Assert.Throws<OptionsValidationException>(() =>
            serviceProvider.GetRequiredService<IOptions<JwtOptions>>().Value);
    }

    private static IConfiguration CreateConfiguration(params KeyValuePair<string, string?>[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:UsersDatabase"] = "Host=localhost;Database=lash;Username=lash;Password=lash",
                ["Jwt:Issuer"] = "lash",
                ["Jwt:Audience"] = "lash-client",
                ["Jwt:Key"] = "a-secure-signing-key-with-at-least-32-characters",
                ["Jwt:ExpiredMinutesTime"] = "15",
                ["Jwt:RefreshTokenLifetimeDays"] = "30",
                ["Jwt:RefreshTokenAbsoluteLifetimeDays"] = "60",
                ["Email:Host"] = "localhost",
                ["Email:FromAddress"] = "noreply@example.com",
                ["RabbitMq:Host"] = "localhost",
                ["RabbitMq:VirtualHost"] = "/",
                ["RabbitMq:UserName"] = "guest",
                ["RabbitMq:Password"] = "guest"
            }.Concat(values)
                .GroupBy(item => item.Key)
                .ToDictionary(group => group.Key, group => group.Last().Value))
            .Build();
}
