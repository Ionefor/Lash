using Lash.Users.Application;
using Lash.Users.Infrastructure;
using Lash.Users.Presentation.Extensions;
using Lash.Users.Presentation.RateLimiting;
using Lash.Web.Http;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Lash.Web.Extensions;

public static class ModuleServiceCollectionExtensions
{
    public static IServiceCollection AddLashModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddControllers()
            .AddUsersPresentation();

        services.AddUsersModule(configuration);
        services.AddLashRateLimiting();
        return services;
    }

    private static IServiceCollection AddUsersModule(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddUsersApplication()
            .AddUsersInfrastructure(configuration);

    private static IServiceCollection AddLashRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, cancellationToken) =>
            {
                context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter);
                return new ValueTask(HttpErrorResponseWriter.WriteRateLimitExceededAsync(
                    context.HttpContext,
                    retryAfter == default ? null : retryAfter,
                    cancellationToken));
            };
            options.AddPolicy(UsersRateLimitPolicies.IdentityPublic, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(15),
                        QueueLimit = 0
            }));
            options.AddPolicy(UsersRateLimitPolicies.Login, context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueLimit = 0
                    }));
            options.AddPolicy(UsersRateLimitPolicies.Registration, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromHours(1),
                        QueueLimit = 0
                    }));
            options.AddPolicy(UsersRateLimitPolicies.Refresh, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });

        return services;
    }
}
