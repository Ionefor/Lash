using Asp.Versioning;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Presentation.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebFlow.AspNetCore.Models;

namespace Lash.Users.Presentation.Extensions;

public static class UsersPresentationExtensions
{
    public static IMvcBuilder AddUsersPresentation(this IMvcBuilder builder)
    {
        builder.AddApplicationPart(typeof(RegistrationController).Assembly);

        builder.Services.AddApiVersioning(options =>
        {
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
            options.ReportApiVersions = true;
        }).AddMvc();

        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.InvalidModelStateResponseFactory = CreateInvalidModelStateResponse);

        builder.Services.PostConfigure<JwtBearerOptions>(
            JwtBearerDefaults.AuthenticationScheme,
            ConfigureJwtErrorResponses);

        return builder;
    }

    private static IActionResult CreateInvalidModelStateResponse(ActionContext context)
    {
        var errors = context.ModelState
            .Where(item => item.Value?.Errors.Count > 0)
            .Select(item => GeneralErrors.ValueIsInvalid(
                string.IsNullOrWhiteSpace(item.Key) ? "request" : item.Key))
            .ToArray();

        return new BadRequestObjectResult(
            Envelope<object?>.Failure(new ErrorList(errors)));
    }

    private static void ConfigureJwtErrorResponses(JwtBearerOptions options)
    {
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();

                if (context.HttpContext.GetEndpoint() is null)
                {
                    return WriteAuthenticationErrorAsync(
                        context.HttpContext,
                        StatusCodes.Status404NotFound,
                        GeneralErrors.NotFound("endpoint", "route").ToErrorList());
                }

                return WriteAuthenticationErrorAsync(
                    context.HttpContext,
                    StatusCodes.Status401Unauthorized,
                    AuthErrors.Unauthorized().ToErrorList());
            },
            OnForbidden = context => WriteAuthenticationErrorAsync(
                context.HttpContext,
                StatusCodes.Status403Forbidden,
                AuthErrors.AccessForbidden().ToErrorList())
        };
    }

    private static Task WriteAuthenticationErrorAsync(
        HttpContext httpContext,
        int statusCode,
        ErrorList errors)
    {
        httpContext.Response.StatusCode = statusCode;
        return httpContext.Response.WriteAsJsonAsync(
            Envelope<object?>.Failure(errors),
            httpContext.RequestAborted);
    }
}
