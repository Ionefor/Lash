using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Constants;
using Lash.Users.Application.Models;
using Lash.Users.Domain;
using Lash.Users.Infrastructure.Authorization;
using Lash.Users.Infrastructure.DbContexts;
using Lash.Users.Infrastructure.Identity;
using Lash.Users.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Lash.Users.Infrastructure.Providers;

public sealed class JwtTokenProvider(
    IOptions<JwtOptions> options,
    UsersDbContext dbContext,
    UserManager<IdentityUserEntity> userManager,
    IPermissionManager permissionManager) : ITokenProvider
{
    private readonly JwtOptions _options = options.Value;

    public async Task<JwtTokenResult> GenerateAccessTokenAsync(
        UserAccount user,
        CancellationToken cancellationToken = default)
    {
        var identityUser = await userManager.FindByIdAsync(user.Id.ToString());
        var roles = identityUser is null ? [] : await userManager.GetRolesAsync(identityUser);
        var permissions = await permissionManager.GetUserPermissionsAsync(user.Id, cancellationToken);
        var jti = Guid.NewGuid();

        var claims = new List<Claim>
        {
            new(AccessTokenClaimTypes.Sub, user.Id.ToString()),
            new(AccessTokenClaimTypes.Jti, jti.ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        }

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(permissions.Select(code => new Claim(CustomClaims.Permission, code)));

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiredMinutesTime),
            signingCredentials: signingCredentials);

        return new JwtTokenResult(new JwtSecurityTokenHandler().WriteToken(token), jti);
    }

    public async Task<Result<string, Error>> GenerateRefreshTokenAsync(
        UserAccount user,
        Guid accessTokenJti,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
        var createdAt = DateTimeOffset.UtcNow;
        var sessionResult = RefreshSession.Create(
            user.Id,
            accessTokenJti,
            RefreshTokenHasher.Hash(refreshToken),
            createdAt,
            createdAt.AddDays(_options.RefreshTokenLifetimeDays));

        if (sessionResult.IsFailure)
        {
            return Result.Failure<string, Error>(sessionResult.Error);
        }

        await dbContext.RefreshSessions.AddAsync(sessionResult.Value, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success<string, Error>(refreshToken);
    }

    public async Task<Result<IReadOnlyList<Claim>, Error>> GetClaimsFromExpiredAccessTokenAsync(
        string jwtToken,
        CancellationToken cancellationToken = default)
    {
        var tokenHandler = new JwtSecurityTokenHandler
        {
            MapInboundClaims = false
        };

        var result = await tokenHandler.ValidateTokenAsync(
            jwtToken,
            TokenValidationParametersFactory.Create(_options, validateLifetime: false));

        if (!result.IsValid)
        {
            return Result.Failure<IReadOnlyList<Claim>, Error>(AuthErrors.TokenInvalid());
        }

        return Result.Success<IReadOnlyList<Claim>, Error>(
            result.ClaimsIdentity.Claims.ToArray());
    }
}
