using Lash.Users.Infrastructure.DbContexts;
using Lash.Users.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Lash.Users.Infrastructure.Extensions;

internal static class UsersIdentityServiceCollectionExtensions
{
    internal static IServiceCollection AddUsersIdentity(this IServiceCollection services)
    {
        services.AddIdentityCore<IdentityUserEntity>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Tokens.EmailConfirmationTokenProvider = EmailCodeTokenProvider<IdentityUserEntity>.ProviderName;
                options.Tokens.PasswordResetTokenProvider = EmailCodeTokenProvider<IdentityUserEntity>.ProviderName;
            })
            .AddRoles<IdentityRoleEntity>()
            .AddEntityFrameworkStores<UsersDbContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<EmailCodeTokenProvider<IdentityUserEntity>>(
                EmailCodeTokenProvider<IdentityUserEntity>.ProviderName);

        return services;
    }
}
