using System.Net;
using System.Text.Json;

namespace Lash.Web.IntegrationTests.Extensions;

public sealed class SwaggerContractTests(LashWebApplicationFactory factory) : IClassFixture<LashWebApplicationFactory>
{
    [Fact]
    public async Task GetSwaggerJson_WhenTestingEnvironment_ReturnsVersionedPathsAndBearerScheme()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var root = document.RootElement;
        Assert.Equal("v1", root.GetProperty("info").GetProperty("version").GetString());
        var paths = root.GetProperty("paths");
        Assert.True(paths.TryGetProperty("/api/v1/auth/me", out _));
        var login = paths.GetProperty("/api/v1/auth/login").GetProperty("post");
        Assert.Empty(login.GetProperty("security").EnumerateArray());
        var bearer = root.GetProperty("components").GetProperty("securitySchemes").GetProperty("bearer");
        Assert.Equal("http", bearer.GetProperty("type").GetString());
        Assert.Equal("bearer", bearer.GetProperty("scheme").GetString());
        Assert.True(root.GetProperty("security")[0].TryGetProperty("bearer", out _));
    }

    [Fact]
    public async Task GetSwaggerUi_WhenTestingEnvironment_ReturnsHtml()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/html", response.Content.Headers.ContentType?.MediaType);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("/swagger/login.js", html, StringComparison.Ordinal);
        Assert.Contains("/swagger/login.css", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetSwaggerLoginScript_WhenTestingEnvironment_ReturnsLoginIntegration()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/swagger/login.js");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var script = await response.Content.ReadAsStringAsync();
        Assert.Contains("preauthorizeApiKey", script, StringComparison.Ordinal);
    }
}
