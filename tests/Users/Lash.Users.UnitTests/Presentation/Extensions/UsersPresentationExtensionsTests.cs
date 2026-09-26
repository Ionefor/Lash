using Asp.Versioning;
using ErrorsFlow.Models;
using Lash.Users.Presentation.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebFlow.AspNetCore.Models;

namespace Lash.Users.UnitTests.Presentation.Extensions;

public sealed class UsersPresentationExtensionsTests
{
    [Fact]
    public void AddUsersPresentation_WhenModelStateIsInvalid_ReturnsErrorEnvelope()
    {
        var services = new ServiceCollection();
        services.AddControllers().AddUsersPresentation();
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value;
        var context = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor(),
            new ModelStateDictionary());
        context.ModelState.AddModelError("email", "Invalid email.");

        var result = Assert.IsType<BadRequestObjectResult>(
            options.InvalidModelStateResponseFactory!(context));

        Assert.IsType<Envelope<object?>>(result.Value);
    }

    [Fact]
    public void AddUsersPresentation_WhenJwtBearerIsConfigured_AddsAuthenticationErrorHandlers()
    {
        var services = new ServiceCollection();
        services.AddControllers().AddUsersPresentation();
        services.AddAuthentication().AddJwtBearer();
        using var provider = services.BuildServiceProvider();

        var options = provider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.NotNull(options.Events.OnChallenge);
        Assert.NotNull(options.Events.OnForbidden);
    }

    [Fact]
    public void AddUsersPresentation_WhenConfigured_ReadsVersionFromUrlSegment()
    {
        var services = new ServiceCollection();
        services.AddControllers().AddUsersPresentation();
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<ApiVersioningOptions>>().Value;

        Assert.IsType<UrlSegmentApiVersionReader>(options.ApiVersionReader);
    }
}
