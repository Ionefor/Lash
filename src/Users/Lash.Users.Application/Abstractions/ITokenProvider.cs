using System.Security.Claims;
using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Application.Models;

namespace Lash.Users.Application.Abstractions;

public interface ITokenProvider
{
    Task<JwtTokenResult> GenerateAccessTokenAsync(
        UserAccount user,
        CancellationToken cancellationToken = default);

    Task<Result<string, Error>> GenerateRefreshTokenAsync(
        UserAccount user,
        Guid accessTokenJti,
        CancellationToken cancellationToken = default);

    Task<Result<string, Error>> GenerateRefreshTokenAsync(
        UserAccount user,
        Guid accessTokenJti,
        DateTimeOffset absoluteExpiresAt,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<Claim>, Error>> GetClaimsFromExpiredAccessTokenAsync(
        string jwtToken,
        CancellationToken cancellationToken = default);
}
