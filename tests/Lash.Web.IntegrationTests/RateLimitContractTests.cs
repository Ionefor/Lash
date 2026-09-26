using System.Net;
using System.Text;
using System.Text.Json;
using ErrorsFlow.Models;
using Lash.Web.Errors;

namespace Lash.Web.IntegrationTests;

public sealed class RateLimitContractTests(LashWebApplicationFactory factory) : IClassFixture<LashWebApplicationFactory>
{
    [Fact]
    public async Task PostLogin_WhenLimitIsExceeded_ReturnsTooManyRequestsEnvelope()
    {
        using var client = factory.CreateClient();

        for (var attempt = 0; attempt < 30; attempt++)
        {
            using var request = new StringContent("{", Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/v1/auth/login", request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        using var limitedRequest = new StringContent("{", Encoding.UTF8, "application/json");
        var limitedResponse = await client.PostAsync("/api/v1/auth/login", limitedRequest);

        Assert.Equal(HttpStatusCode.TooManyRequests, limitedResponse.StatusCode);
        await AssertRateLimitErrorAsync(limitedResponse);
    }

    [Fact]
    public async Task PostRegistration_WhenLimitIsExceeded_ReturnsTooManyRequestsEnvelope()
    {
        using var client = factory.CreateClient();

        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var request = new StringContent("{", Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/v1/registration/client", request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        using var limitedRequest = new StringContent("{", Encoding.UTF8, "application/json");
        var limitedResponse = await client.PostAsync("/api/v1/registration/client", limitedRequest);

        Assert.Equal(HttpStatusCode.TooManyRequests, limitedResponse.StatusCode);
        await AssertRateLimitErrorAsync(limitedResponse);
    }

    private static async Task AssertRateLimitErrorAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var error = document.RootElement.GetProperty("errors")[0];

        Assert.Equal(WebErrorCodes.RequestRateLimited, error.GetProperty("code").GetString());
        Assert.Equal((int)ErrorType.Failure, error.GetProperty("type").GetInt32());
        Assert.Equal("request", error.GetProperty("target").GetString());
    }
}
