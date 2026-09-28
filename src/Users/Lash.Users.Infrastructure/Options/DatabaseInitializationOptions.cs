namespace Lash.Users.Infrastructure.Options;

public sealed class DatabaseInitializationOptions
{
    public const string SectionName = ConfigurationSectionNames.DatabaseInitialization;

    public bool ApplyMigrationsOnStartup { get; init; }
    public bool ApplySeedOnStartup { get; init; }
}
