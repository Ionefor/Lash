using Lash.Web.Http;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;

namespace Lash.Web.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseLashWeb(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (httpContext, _, exception) =>
            {
                if (exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError)
                {
                    return LogEventLevel.Error;
                }

                return httpContext.Response.StatusCode >= StatusCodes.Status400BadRequest
                    ? LogEventLevel.Debug
                    : LogEventLevel.Information;
            };
        });
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
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseRateLimiter();
        app.UseAuthorization();
        return app;
    }

    public static WebApplication MapLashEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        }).AllowAnonymous();
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("ready")
        }).AllowAnonymous();
        app.MapControllers();
        return app;
    }
}
