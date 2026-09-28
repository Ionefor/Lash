namespace Lash.Users.Infrastructure.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = ConfigurationSectionNames.RabbitMq;

    public string Host { get; init; } = string.Empty;
    public ushort Port { get; init; } = 5672;
    public string VirtualHost { get; init; } = "/";
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
