using Lash.Users.Infrastructure.Authorization;
using Lash.Users.Infrastructure.Options;

namespace Lash.Users.UnitTests.Infrastructure.Authorization;

public sealed class TokenValidationParametersFactoryTests
{
    [Fact]
    public void Create_WhenLifetimeValidationDisabled_PreservesAllOtherValidationRules()
    {
        var options = new JwtOptions
        {
            Issuer = "lash",
            Audience = "lash-client",
            Key = "a-secure-signing-key-with-at-least-32-characters"
        };

        var parameters = TokenValidationParametersFactory.Create(options, validateLifetime: false);

        Assert.Equal(options.Issuer, parameters.ValidIssuer);
        Assert.Equal(options.Audience, parameters.ValidAudience);
        Assert.True(parameters.ValidateIssuer);
        Assert.True(parameters.ValidateAudience);
        Assert.True(parameters.ValidateIssuerSigningKey);
        Assert.False(parameters.ValidateLifetime);
        Assert.Equal(TimeSpan.Zero, parameters.ClockSkew);
    }
}
