namespace Lash.Users.Infrastructure.Options;

public sealed class IdentityEmailRateLimitOptions
{
    public const string SectionName = ConfigurationSectionNames.IdentityEmailRateLimit;

    public int EmailConfirmationLimit { get; init; } = 3;
    public int EmailConfirmationWindowMinutes { get; init; } = 15;
    public int PasswordResetLimit { get; init; } = 3;
    public int PasswordResetWindowMinutes { get; init; } = 60;
}
