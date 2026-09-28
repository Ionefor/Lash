using Lash.Users.Application.Abstractions;
using Lash.Users.Infrastructure.DbContexts;
using Lash.Users.Infrastructure.Messaging;
using Lash.Users.Infrastructure.Options;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lash.Users.Infrastructure.Extensions;

internal static class UsersMessagingServiceCollectionExtensions
{
    internal static IServiceCollection AddUsersMessaging(this IServiceCollection services)
    {
        services.AddMassTransit(configurator =>
        {
            configurator.AddConsumer<EmailConfirmationRequestedConsumer>();
            configurator.AddConsumer<PasswordResetRequestedConsumer>();
            configurator.AddEntityFrameworkOutbox<UsersDbContext>(outbox =>
            {
                outbox.UsePostgres();
                outbox.UseBusOutbox();
            });
            configurator.UsingRabbitMq((context, bus) =>
            {
                var rabbitMqOptions = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                bus.Host(rabbitMqOptions.Host, rabbitMqOptions.Port, rabbitMqOptions.VirtualHost, host =>
                {
                    host.Username(rabbitMqOptions.UserName);
                    host.Password(rabbitMqOptions.Password);
                });
                bus.ReceiveEndpoint("users-email-confirmation", endpoint =>
                {
                    endpoint.ConfigureConsumer<EmailConfirmationRequestedConsumer>(context);
                    endpoint.UseEntityFrameworkOutbox<UsersDbContext>(context);
                    endpoint.UseMessageRetry(retry => retry.Intervals(
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(30)));
                });
                bus.ReceiveEndpoint("users-password-reset", endpoint =>
                {
                    endpoint.ConfigureConsumer<PasswordResetRequestedConsumer>(context);
                    endpoint.UseEntityFrameworkOutbox<UsersDbContext>(context);
                    endpoint.UseMessageRetry(retry => retry.Intervals(
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(30)));
                });
            });
        });
        services.AddScoped<IUsersEventPublisher, MassTransitUsersEventPublisher>();

        return services;
    }
}
