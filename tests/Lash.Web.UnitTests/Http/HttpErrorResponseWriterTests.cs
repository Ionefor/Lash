using System.Text.Json;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Web.Errors;
using Lash.Web.Http;
using Microsoft.AspNetCore.Http;

namespace Lash.Web.UnitTests.Http;

public sealed class HttpErrorResponseWriterTests
{
    [Fact]
    public async Task WriteStatusCodeErrorAsync_WhenRouteIsNotFound_ReturnsNotFoundEnvelope()
    {
        var context = CreateContext(StatusCodes.Status404NotFound);

        await HttpErrorResponseWriter.WriteStatusCodeErrorAsync(context, CancellationToken.None);

        var error = await ReadFirstErrorAsync(context);
        Assert.Equal(GeneralErrorCodes.NotFound, error.Code);
        Assert.Equal(ErrorType.NotFound, error.Type);
        Assert.Equal("route", error.Target);
    }

    [Fact]
    public async Task WriteStatusCodeErrorAsync_WhenMethodIsNotAllowed_ReturnsMethodNotAllowedEnvelope()
    {
        var context = CreateContext(StatusCodes.Status405MethodNotAllowed);

        await HttpErrorResponseWriter.WriteStatusCodeErrorAsync(context, CancellationToken.None);

        var error = await ReadFirstErrorAsync(context);
        Assert.Equal(WebErrorCodes.MethodNotAllowed, error.Code);
        Assert.Equal(ErrorType.Failure, error.Type);
        Assert.Equal("request", error.Target);
    }

    [Fact]
    public async Task WriteRateLimitExceededAsync_WhenRetryAfterIsKnown_ReturnsEnvelopeAndRetryAfterHeader()
    {
        var context = CreateContext(StatusCodes.Status429TooManyRequests);

        await HttpErrorResponseWriter.WriteRateLimitExceededAsync(context, TimeSpan.FromSeconds(12.1), CancellationToken.None);

        var error = await ReadFirstErrorAsync(context);
        Assert.Equal(WebErrorCodes.RequestRateLimited, error.Code);
        Assert.Equal(ErrorType.Failure, error.Type);
        Assert.Equal("request", error.Target);
        Assert.Equal("13", context.Response.Headers.RetryAfter);
    }

    private static DefaultHttpContext CreateContext(int statusCode)
    {
        var context = new DefaultHttpContext();
        context.Response.StatusCode = statusCode;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<Error> ReadFirstErrorAsync(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        using var response = await JsonDocument.ParseAsync(context.Response.Body);
        var error = response.RootElement.GetProperty("errors")[0];
        return Error.Create(
            error.GetProperty("code").GetString()!,
            error.GetProperty("message").GetString()!,
            (ErrorType)error.GetProperty("type").GetInt32(),
            error.GetProperty("target").GetString()!);
    }
}
