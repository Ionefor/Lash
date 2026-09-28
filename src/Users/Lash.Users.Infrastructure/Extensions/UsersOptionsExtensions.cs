using System.Net.Mail;
using Lash.Users.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lash.Users.Infrastructure.Extensions;

internal static class UsersOptionsExtensions
{
    internal static IServiceCollection AddUsersOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var databaseInitializationOptions = configuration
            .GetSection(DatabaseInitializationOptions.SectionName)
            .Get<DatabaseInitializationOptions>()
            ?? new DatabaseInitializationOptions();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Jwt:Audience is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Key), "Jwt:Key is required.")
            .Validate(options => options.Key.Length >= 32, "Jwt:Key must contain at least 32 characters.")
            .Validate(options => options.ExpiredMinutesTime > 0, "Jwt:ExpiredMinutesTime must be positive.")
            .Validate(options => options.RefreshTokenLifetimeDays > 0, "Jwt:RefreshTokenLifetimeDays must be positive.")
            .Validate(options => options.RefreshTokenAbsoluteLifetimeDays >= options.RefreshTokenLifetimeDays, "Jwt:RefreshTokenAbsoluteLifetimeDays must be at least Jwt:RefreshTokenLifetimeDays.")
            .ValidateOnStart();

        services.AddOptions<RolePermissionOptions>()
            .Bind(configuration.GetSection(RolePermissionOptions.SectionName))
            .ValidateForSeed(databaseInitializationOptions.ApplySeedOnStartup);
        services.AddOptions<AdminOptions>()
            .Bind(configuration.GetSection(AdminOptions.SectionName))
            .ValidateForSeed(databaseInitializationOptions.ApplySeedOnStartup);
        services.AddOptions<DatabaseInitializationOptions>()
            .Bind(configuration.GetSection(DatabaseInitializationOptions.SectionName));
        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Host), "Email:Host is required.")
            .Validate(options => options.Port is > 0 and <= 65_535, "Email:Port must be between 1 and 65535.")
            .Validate(options => HasValidEmailAddress(options.FromAddress), "Email:FromAddress must be a valid email address.")
            .ValidateOnStart();
        services.AddOptions<IdentityEmailRateLimitOptions>()
            .Bind(configuration.GetSection(IdentityEmailRateLimitOptions.SectionName))
            .Validate(options => options.EmailConfirmationLimit > 0, "IdentityEmailRateLimit:EmailConfirmationLimit must be positive.")
            .Validate(options => options.EmailConfirmationWindowMinutes > 0, "IdentityEmailRateLimit:EmailConfirmationWindowMinutes must be positive.")
            .Validate(options => options.PasswordResetLimit > 0, "IdentityEmailRateLimit:PasswordResetLimit must be positive.")
            .Validate(options => options.PasswordResetWindowMinutes > 0, "IdentityEmailRateLimit:PasswordResetWindowMinutes must be positive.")
            .Validate(options => options.CodeVerificationAttemptLimit > 0, "IdentityEmailRateLimit:CodeVerificationAttemptLimit must be positive.")
            .ValidateOnStart();
        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Host), "RabbitMq:Host is required.")
            .Validate(options => options.Port > 0, "RabbitMq:Port must be positive.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.VirtualHost), "RabbitMq:VirtualHost is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.UserName), "RabbitMq:UserName is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Password), "RabbitMq:Password is required.")
            .ValidateOnStart();

        return services;
    }

    private static bool HasValidEmailAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return false;
        }

        try
        {
            return string.Equals(
                new MailAddress(address).Address,
                address,
                StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
