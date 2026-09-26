using System.Net;
using System.Text;
using System.Text.Json;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;

namespace Lash.Web.IntegrationTests;

public sealed class HttpContractTests(LashWebApplicationFactory factory) : IClassFixture<LashWebApplicationFactory>
{
    [Fact]
    public async Task GetCurrentUser_WhenAccessTokenIsMissing_ReturnsUnauthorizedEnvelope()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await AssertErrorAsync(response, AuthErrorCodes.Unauthorized, ErrorType.Unauthorized, null);
    }

    [Fact]
    public async Task PostLogin_WhenJsonIsMalformed_ReturnsBadRequestEnvelope()
    {
        using var client = factory.CreateClient();
        using var request = new StringContent("{", Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/v1/auth/login", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertErrorAsync(response, GeneralErrorCodes.ValueIsInvalid, ErrorType.Validation, "$");
    }

    [Fact]
    public async Task GetUnknownEndpoint_ReturnsNotFoundEnvelope()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/unknown");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertErrorAsync(response, GeneralErrorCodes.NotFound, ErrorType.NotFound, "route");
    }

    [Fact]
    public async Task GetLiveness_WhenAnonymous_ReturnsHealthy()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task AssertErrorAsync(
        HttpResponseMessage response,
        string expectedCode,
        ErrorType expectedType,
        string? expectedTarget)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var error = document.RootElement.GetProperty("errors")[0];

        Assert.Equal(expectedCode, error.GetProperty("code").GetString());
        Assert.Equal((int)expectedType, error.GetProperty("type").GetInt32());
        Assert.Equal(expectedTarget, error.GetProperty("target").GetString());
    }
}
