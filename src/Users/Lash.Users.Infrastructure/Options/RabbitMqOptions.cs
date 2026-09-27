namespace Lash.Users.Infrastructure.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public bool Enabled { get; init; } = true;
    public string Host { get; init; } = "localhost";
    public string VirtualHost { get; init; } = "/";
    public string UserName { get; init; } = "guest";
    public string Password { get; init; } = "guest";
}
