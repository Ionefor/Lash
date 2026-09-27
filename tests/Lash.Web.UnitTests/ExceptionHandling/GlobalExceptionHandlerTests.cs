using System.Text.Json;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
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
        using var document = JsonDocument.Parse(response);
        var error = document.RootElement.GetProperty("errors")[0];

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal(GeneralErrorCodes.InternalServer, error.GetProperty("code").GetString());
        Assert.Equal((int)ErrorType.InternalServer, error.GetProperty("type").GetInt32());
        Assert.Null(error.GetProperty("target").GetString());
        Assert.DoesNotContain("Sensitive database detail", response, StringComparison.Ordinal);
    }
}
