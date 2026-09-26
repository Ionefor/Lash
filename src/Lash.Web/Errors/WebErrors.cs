using ErrorsFlow;
using ErrorsFlow.Models;

namespace Lash.Web.Errors;

public static class WebErrors
{
    public static Error RequestRateLimited() => ErrorFactory.Create(
        WebErrorCodes.RequestRateLimited,
        "Too many requests. Please try again later.",
        ErrorType.Failure,
        "request");

    public static Error MethodNotAllowed() => ErrorFactory.Create(
        WebErrorCodes.MethodNotAllowed,
        "The HTTP method is not allowed for this endpoint.",
        ErrorType.Failure,
        "request");
}
