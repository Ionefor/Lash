namespace Lash.Users.Infrastructure.Options;

public sealed class AdminOptions
{
    public const string SectionName = ConfigurationSectionNames.Admin;

    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
