using Asp.Versioning;
using ErrorsFlow;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Web.Authorization;
using Lash.Web.Http;
using Lash.Web.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using WebFlow.AspNetCore.Models;

namespace Lash.Web.Extensions;

public static class HttpServiceCollectionExtensions
{
    public static IMvcBuilder AddLashHttp(this IServiceCollection services)
    {
        var mvcBuilder = services.AddControllers();

        services.AddApiVersioning(options =>
        {
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
            options.ReportApiVersions = true;
        }).AddMvc().AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Lash API", Version = "v1" });
            options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter an access token."
            });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = []
            });
            options.OperationFilter<AllowAnonymousOperationFilter>();
        });
        services.Configure<ApiBehaviorOptions>(options =>
            options.InvalidModelStateResponseFactory = CreateInvalidModelStateResponse);
        services.PostConfigure<JwtBearerOptions>(
            JwtBearerDefaults.AuthenticationScheme,
            ConfigureJwtChallengeResponse);
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, ErrorEnvelopeAuthorizationMiddlewareResultHandler>();

        return mvcBuilder;
    }

    private static IActionResult CreateInvalidModelStateResponse(ActionContext context)
    {
        var errors = context.ModelState
            .SelectMany(item => item.Value?.Errors.Select(error => CreateModelStateError(item.Key, error)) ?? [])
            .ToArray();

        return new BadRequestObjectResult(
            Envelope<object?>.Failure(new ErrorList(errors)));
    }

    private static Error CreateModelStateError(string key, ModelError error)
    {
        var target = string.IsNullOrWhiteSpace(key) ? "request" : key;
        var message = error.Exception is null && !string.IsNullOrWhiteSpace(error.ErrorMessage)
            ? error.ErrorMessage
            : "Value is invalid.";

        return ErrorFactory.Create(
            GeneralErrorCodes.ValueIsInvalid,
            message,
            ErrorType.Validation,
            target);
    }

    private static void ConfigureJwtChallengeResponse(JwtBearerOptions options)
    {
        options.Events ??= new JwtBearerEvents();

        var previousOnChallenge = options.Events.OnChallenge;
        options.Events.OnChallenge = async context =>
        {
            if (previousOnChallenge is not null)
            {
                await previousOnChallenge(context);
            }

            if (context.Handled || context.Response.HasStarted)
            {
                return;
            }

            context.HandleResponse();
            var error = context.HttpContext.GetEndpoint() is null
                ? GeneralErrors.NotFound("endpoint", "route")
                : AuthErrors.Unauthorized();
            context.Response.StatusCode = error.Type is ErrorType.NotFound
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status401Unauthorized;
            await HttpErrorResponseWriter.WriteErrorAsync(
                context.HttpContext,
                error,
                context.HttpContext.RequestAborted);
        };
    }
}
