namespace Lash.Users.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Audience { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public int ExpiredMinutesTime { get; init; } = 60;
    public int RefreshTokenLifetimeDays { get; init; } = 60;
}
