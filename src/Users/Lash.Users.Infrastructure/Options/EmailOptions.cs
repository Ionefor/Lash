namespace Lash.Users.Infrastructure.Options;

public sealed class EmailOptions
{
    public const string SectionName = ConfigurationSectionNames.Email;
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromAddress { get; init; } = string.Empty;
    public bool UseSsl { get; init; } = true;
}
