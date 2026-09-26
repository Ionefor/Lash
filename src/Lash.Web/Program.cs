using Lash.Users.Application;
using Lash.Users.Infrastructure;
using Lash.Users.Presentation.Extensions;
using Lash.Users.Presentation.RateLimiting;
using Lash.Web.ExceptionHandling;
using Lash.Web.Http;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddUsersPresentation();

builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddRateLimiter(options =>
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
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
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

builder.Services
    .AddUsersApplication()
    .AddUsersInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages(async statusCodeContext =>
{
    var statusCode = statusCodeContext.HttpContext.Response.StatusCode;
    if (statusCode is StatusCodes.Status404NotFound or StatusCodes.Status405MethodNotAllowed)
    {
        await HttpErrorResponseWriter.WriteStatusCodeErrorAsync(
            statusCodeContext.HttpContext,
            statusCodeContext.HttpContext.RequestAborted);
    }
});

if (app.Configuration.GetValue<bool>("DatabaseInitialization:ApplyMigrationsOnStartup"))
{
    await app.Services.MigrateUsersDatabaseAsync();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live").AllowAnonymous();
app.MapControllers();

app.Run();

public partial class Program
{
}
