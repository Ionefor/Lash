using System.Text.Json;
using Asp.Versioning;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Web.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lash.Web.UnitTests.Extensions;

public sealed class HttpServiceCollectionExtensionsTests
{
    [Fact]
    public void AddLashHttp_WhenModelStateContainsMultipleErrors_ReturnsSafeErrorForEachError()
    {
        var services = new ServiceCollection();
        services.AddLashHttp();
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value;
        var context = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor(),
            new ModelStateDictionary());
        context.ModelState.AddModelError("email", "Email is required.");
        context.ModelState.AddModelError("email", "Email format is invalid.");
        context.ModelState.TryAddModelException("age", new FormatException("Sensitive conversion detail."));

        var result = Assert.IsType<BadRequestObjectResult>(
            options.InvalidModelStateResponseFactory!(context));
        var response = JsonSerializer.Serialize(result.Value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        using var document = JsonDocument.Parse(response);
        var errors = document.RootElement.GetProperty("errors");

        Assert.Equal(3, errors.GetArrayLength());
        Assert.All(errors.EnumerateArray(), error =>
        {
            Assert.Equal(GeneralErrorCodes.ValueIsInvalid, error.GetProperty("code").GetString());
            Assert.Equal((int)ErrorType.Validation, error.GetProperty("type").GetInt32());
        });
        var messagesByTarget = errors.EnumerateArray()
            .GroupBy(error => error.GetProperty("target").GetString()!)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.GetProperty("message").GetString()!).ToArray());

        Assert.Equal(["Email is required.", "Email format is invalid."], messagesByTarget["email"]);
        Assert.Equal(["Value is invalid."], messagesByTarget["age"]);
        Assert.DoesNotContain("Sensitive conversion detail.", response, StringComparison.Ordinal);
    }

    [Fact]
    public void AddLashHttp_WhenJwtEventsAlreadyConfigured_PreservesUnrelatedCallbacks()
    {
        var onTokenValidated = new Func<TokenValidatedContext, Task>(_ => Task.CompletedTask);
        var onMessageReceived = new Func<MessageReceivedContext, Task>(_ => Task.CompletedTask);
        var services = new ServiceCollection();
        services.AddAuthentication().AddJwtBearer(options => options.Events = new JwtBearerEvents
        {
            OnTokenValidated = onTokenValidated,
            OnMessageReceived = onMessageReceived
        });
        services.AddLashHttp();
        using var provider = services.BuildServiceProvider();

        var options = provider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.Same(onTokenValidated, options.Events.OnTokenValidated);
        Assert.Same(onMessageReceived, options.Events.OnMessageReceived);
        Assert.NotNull(options.Events.OnChallenge);
    }

    [Fact]
    public void AddLashHttp_WhenConfigured_ReadsVersionFromUrlSegment()
    {
        var services = new ServiceCollection();
        services.AddLashHttp();
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<ApiVersioningOptions>>().Value;

        Assert.IsType<UrlSegmentApiVersionReader>(options.ApiVersionReader);
    }
}
