using ErrorsFlow.Errors;
using Lash.Web.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Lash.Web.Authorization;

public sealed class ErrorEnvelopeAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly IAuthorizationMiddlewareResultHandler _defaultHandler = new AuthorizationMiddlewareResultHandler();

    public Task HandleAsync(
        RequestDelegate next,
        HttpContext httpContext,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (!authorizeResult.Forbidden)
        {
            return _defaultHandler.HandleAsync(next, httpContext, policy, authorizeResult);
        }

        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        return HttpErrorResponseWriter.WriteErrorAsync(
            httpContext,
            AuthErrors.AccessForbidden(),
            httpContext.RequestAborted);
    }
}
