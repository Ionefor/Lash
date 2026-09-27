using System.Text.Json;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace Lash.Web.UnitTests.Authorization;

public sealed class ErrorEnvelopeAuthorizationMiddlewareResultHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenAuthorizationIsForbidden_WritesForbiddenEnvelopeWithoutInvokingNext()
    {
        var context = new DefaultHttpContext();
        await using var body = new MemoryStream();
        context.Response.Body = body;
        var nextCalled = false;
        var handler = new ErrorEnvelopeAuthorizationMiddlewareResultHandler();
        var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

        await handler.HandleAsync(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            context,
            policy,
            PolicyAuthorizationResult.Forbid());

        body.Position = 0;
        using var document = await JsonDocument.ParseAsync(body);
        var error = document.RootElement.GetProperty("errors")[0];

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.Equal(AuthErrorCodes.AccessForbidden, error.GetProperty("code").GetString());
        Assert.Equal((int)ErrorType.Forbidden, error.GetProperty("type").GetInt32());
    }
}
