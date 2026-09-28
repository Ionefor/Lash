using Lash.Users.Application.Abstractions;
using Lash.Users.Infrastructure.Authorization;
using Lash.Users.Infrastructure.Identity;
using Lash.Users.Infrastructure.Messaging;
using Lash.Users.Infrastructure.Persistence;
using Lash.Users.Infrastructure.Providers;
using Lash.Users.Infrastructure.Seeding;
using Microsoft.Extensions.DependencyInjection;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Infrastructure.Extensions;

internal static class UsersCoreServicesServiceCollectionExtensions
{
    internal static IServiceCollection AddUsersCoreServices(this IServiceCollection services)
    {
        services.AddScoped<RolesSeeder>();
        services.AddScoped<PermissionsSeeder>();
        services.AddScoped<AdminSeeder>();
        services.AddScoped<ISeederRegistry, UsersSeederRegistry>();
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<PermissionManager>();
        services.AddScoped<RolePermissionManager>();
        services.AddScoped<IPermissionManager>(provider => provider.GetRequiredService<PermissionManager>());
        services.AddScoped<IRefreshSessionManager, RefreshSessionManager>();
        services.AddScoped<IUserSessionLock, UserSessionLock>();
        services.AddScoped<IUserAccountService, UserAccountService>();
        services.AddScoped<ITokenProvider, JwtTokenProvider>();
        services.AddSingleton<IIdentityEmailTemplateFactory, IdentityEmailTemplateFactory>();
        services.AddSingleton<IEmailTransport, SmtpEmailTransport>();
        services.AddScoped<IEmailConfirmationEmailSender, EmailConfirmationSender>();
        services.AddScoped<IPasswordResetSender, PasswordResetSender>();
        services.AddScoped<IIdentityEmailRequestStore, PostgresIdentityEmailRequestStore>();
        services.AddScoped<IIdentityEmailRequestLimiter, IdentityEmailRequestLimiter>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
