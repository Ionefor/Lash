using System.Text;
using Lash.Users.Infrastructure.Options;
using Microsoft.IdentityModel.Tokens;

namespace Lash.Users.Infrastructure.Authorization;

public static class TokenValidationParametersFactory
{
    public static TokenValidationParameters Create(
        JwtOptions options,
        bool validateLifetime = true) =>
        new()
        {
            ValidIssuer = options.Issuer,
            ValidAudience = options.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = validateLifetime,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };
}
