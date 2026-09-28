using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace Lash.Web.IntegrationTests;

public sealed class ContainerizedLashWebApplicationFactory : WebApplicationFactory<Program>
{
    private PostgreSqlContainer? _postgres;
    private RabbitMqContainer? _rabbitMq;

    public async Task StartAsync()
    {
        _postgres = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("lash")
            .WithUsername("lash")
            .WithPassword("lash")
            .Build();
        _rabbitMq = new RabbitMqBuilder("rabbitmq:3.13-management-alpine")
            .WithUsername("lash")
            .WithPassword("lash")
            .Build();

        await _postgres.StartAsync();
        await _rabbitMq.StartAsync();
    }

    public async Task StopAsync()
    {
        Dispose();

        if (_rabbitMq is not null)
        {
            await _rabbitMq.DisposeAsync();
        }

        if (_postgres is not null)
        {
            await _postgres.DisposeAsync();
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:UsersDatabase"] = GetPostgres().GetConnectionString(),
                ["RabbitMq:Host"] = GetRabbitMq().Hostname,
                ["RabbitMq:Port"] = GetRabbitMq().GetMappedPublicPort(5672).ToString(),
                ["RabbitMq:UserName"] = "lash",
                ["RabbitMq:Password"] = "lash",
                ["DatabaseInitialization:ApplyMigrationsOnStartup"] = "true",
                ["DatabaseInitialization:ApplySeedOnStartup"] = "false"
            }));
    }

    private PostgreSqlContainer GetPostgres() =>
        _postgres ?? throw new InvalidOperationException("PostgreSQL container has not started.");

    private RabbitMqContainer GetRabbitMq() =>
        _rabbitMq ?? throw new InvalidOperationException("RabbitMQ container has not started.");
}
