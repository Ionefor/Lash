using ErrorsFlow.Errors;
using Lash.Web.Errors;
using Microsoft.AspNetCore.Http;
using WebFlow.AspNetCore.Models;

namespace Lash.Web.Http;

public static class HttpErrorResponseWriter
{
    public static Task WriteStatusCodeErrorAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var error = httpContext.Response.StatusCode switch
        {
            StatusCodes.Status404NotFound => GeneralErrors.NotFound("endpoint", "route"),
            StatusCodes.Status405MethodNotAllowed => WebErrors.MethodNotAllowed(),
            _ => throw new InvalidOperationException($"Unsupported status code: {httpContext.Response.StatusCode}.")
        };

        return WriteErrorAsync(httpContext, error, cancellationToken);
    }

    public static Task WriteRateLimitExceededAsync(
        HttpContext httpContext,
        TimeSpan? retryAfter,
        CancellationToken cancellationToken)
    {
        if (retryAfter is { } delay)
        {
            httpContext.Response.Headers.RetryAfter = Math.Ceiling(delay.TotalSeconds).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return WriteErrorAsync(httpContext, WebErrors.RequestRateLimited(), cancellationToken);
    }

    public static Task WriteErrorAsync(
        HttpContext httpContext,
        ErrorsFlow.Models.Error error,
        CancellationToken cancellationToken)
    {
        httpContext.Response.ContentType = "application/json";
        return httpContext.Response.WriteAsJsonAsync(
            Envelope<object?>.Failure(error.ToErrorList()),
            cancellationToken);
    }
}
