using System.Security.Claims;
using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Application.Models;
using Lash.Users.Domain;

namespace Lash.Users.Application.Abstractions;

public interface ITokenProvider
{
    Task<JwtTokenResult> GenerateAccessTokenAsync(
        User user,
        CancellationToken cancellationToken = default);

    Task<Result<string, Error>> GenerateRefreshTokenAsync(
        User user,
        Guid accessTokenJti,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<Claim>, Error>> GetClaimsFromExpiredAccessTokenAsync(
        string jwtToken,
        CancellationToken cancellationToken = default);
}
