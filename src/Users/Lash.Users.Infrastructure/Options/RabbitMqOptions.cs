namespace Lash.Users.Infrastructure.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; init; } = string.Empty;
    public string VirtualHost { get; init; } = "/";
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
