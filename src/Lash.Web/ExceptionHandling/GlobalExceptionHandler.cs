using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Microsoft.AspNetCore.Diagnostics;
using WebFlow.AspNetCore.Models;

namespace Lash.Web.ExceptionHandling;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception while processing request {RequestPath}", httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            Envelope<object?>.Failure(GeneralErrors.InternalServer().ToErrorList()),
            cancellationToken);

        return true;
    }
}
