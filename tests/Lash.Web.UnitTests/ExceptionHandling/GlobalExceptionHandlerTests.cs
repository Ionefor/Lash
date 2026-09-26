using Lash.Web.ExceptionHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lash.Web.UnitTests.ExceptionHandling;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WhenUnexpectedExceptionOccurs_ReturnsSafeInternalServerEnvelope()
    {
        var context = new DefaultHttpContext();
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var handler = new GlobalExceptionHandler(Mock.Of<ILogger<GlobalExceptionHandler>>());

        var handled = await handler.TryHandleAsync(
            context,
            new InvalidOperationException("Sensitive database detail"),
            CancellationToken.None);

        responseBody.Position = 0;
        using var reader = new StreamReader(responseBody);
        var response = await reader.ReadToEndAsync();

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Contains("server.internal", response, StringComparison.Ordinal);
        Assert.DoesNotContain("Sensitive database detail", response, StringComparison.Ordinal);
    }
}
