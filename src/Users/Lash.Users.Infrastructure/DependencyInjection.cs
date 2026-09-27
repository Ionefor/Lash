using Lash.Users.Application.Abstractions;
using Lash.Users.Infrastructure.Authorization;
using Lash.Users.Infrastructure.DbContexts;
using Lash.Users.Infrastructure.Identity;
using Lash.Users.Infrastructure.Options;
using Lash.Users.Infrastructure.Messaging;
using MassTransit;
using Lash.Users.Infrastructure.Persistence;
using Lash.Users.Infrastructure.Providers;
using Lash.Users.Infrastructure.Seeding;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("UsersDatabase")
            ?? throw new InvalidOperationException("Connection string 'UsersDatabase' is not configured.");

        services.AddDbContext<UsersDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());

        services.AddIdentityCore<IdentityUserEntity>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRoleEntity>()
            .AddEntityFrameworkStores<UsersDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<RolesSeeder>();
        services.AddScoped<PermissionsSeeder>();
        services.AddScoped<AdminSeeder>();
        services.AddScoped<ISeeder>(provider => provider.GetRequiredService<RolesSeeder>());
        services.AddScoped<ISeeder>(provider => provider.GetRequiredService<PermissionsSeeder>());
        services.AddScoped<ISeeder>(provider => provider.GetRequiredService<AdminSeeder>());
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<PermissionManager>();
        services.AddScoped<RolePermissionManager>();
        services.AddScoped<IPermissionManager>(provider => provider.GetRequiredService<PermissionManager>());
        services.AddScoped<IRefreshSessionManager, RefreshSessionManager>();
        services.AddScoped<IUserSessionLock, UserSessionLock>();
        services.AddScoped<IUserAccountService, UserAccountService>();
        services.AddScoped<ITokenProvider, JwtTokenProvider>();
        services.AddScoped<IEmailConfirmationEmailSender, EmailConfirmationSender>();
        services.AddScoped<IPasswordResetSender, PasswordResetSender>();
        services.AddScoped<IIdentityEmailRequestLimiter, IdentityEmailRequestLimiter>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Jwt:Audience is required.")
            .Validate(options => options.Key.Length >= 32, "Jwt:Key must contain at least 32 characters.")
            .Validate(options => options.ExpiredMinutesTime > 0, "Jwt:ExpiredMinutesTime must be positive.")
            .Validate(options => options.RefreshTokenLifetimeDays > 0, "Jwt:RefreshTokenLifetimeDays must be positive.")
            .Validate(options => options.RefreshTokenAbsoluteLifetimeDays >= options.RefreshTokenLifetimeDays, "Jwt:RefreshTokenAbsoluteLifetimeDays must be at least Jwt:RefreshTokenLifetimeDays.")
            .ValidateOnStart();

        var databaseInitializationOptions = configuration
            .GetSection(DatabaseInitializationOptions.SectionName)
            .Get<DatabaseInitializationOptions>()
            ?? new DatabaseInitializationOptions();

        services.AddOptions<RolePermissionOptions>()
            .Bind(configuration.GetSection(RolePermissionOptions.SectionName))
            .ValidateForSeed(databaseInitializationOptions.ApplySeedOnStartup);
        services.AddOptions<AdminOptions>()
            .Bind(configuration.GetSection(AdminOptions.SectionName))
            .ValidateForSeed(databaseInitializationOptions.ApplySeedOnStartup);
        services.AddOptions<DatabaseInitializationOptions>()
            .Bind(configuration.GetSection(DatabaseInitializationOptions.SectionName));
        services.AddOptions<EmailOptions>().Bind(configuration.GetSection(EmailOptions.SectionName)).Validate(options => !string.IsNullOrWhiteSpace(options.Host) && !string.IsNullOrWhiteSpace(options.FromAddress), "Email SMTP settings are required.").ValidateOnStart();

        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Host), "RabbitMq:Host is required.")
            .ValidateOnStart();

        services.AddMassTransit(configurator =>
        {
            configurator.AddConsumer<EmailConfirmationRequestedConsumer>();
            configurator.AddConsumer<PasswordResetRequestedConsumer>();
            configurator.AddEntityFrameworkOutbox<UsersDbContext>(outbox =>
            {
                outbox.UsePostgres();
                outbox.UseBusOutbox();
            });
            configurator.UsingRabbitMq((context, bus) =>
            {
                var rabbitMqOptions = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                bus.Host(rabbitMqOptions.Host, rabbitMqOptions.VirtualHost, host =>
                {
                    host.Username(rabbitMqOptions.UserName);
                    host.Password(rabbitMqOptions.Password);
                });
                bus.ReceiveEndpoint("users-email-confirmation", endpoint =>
                {
                    endpoint.ConfigureConsumer<EmailConfirmationRequestedConsumer>(context);
                    endpoint.UseEntityFrameworkOutbox<UsersDbContext>(context);
                    endpoint.UseMessageRetry(retry => retry.Intervals(
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(30)));
                });
                bus.ReceiveEndpoint("users-password-reset", endpoint =>
                {
                    endpoint.ConfigureConsumer<PasswordResetRequestedConsumer>(context);
                    endpoint.UseEntityFrameworkOutbox<UsersDbContext>(context);
                    endpoint.UseMessageRetry(retry => retry.Intervals(
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(30)));
                });
            });
        });
        services.AddScoped<IUsersEventPublisher, MassTransitUsersRegistrationEventPublisher>();

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? new JwtOptions();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = TokenValidationParametersFactory.Create(jwtOptions);
            });

        services.AddAuthorization(options =>
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionRequirementHandler>();

        return services;
    }
}
