using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Domain;

namespace Lash.Users.Application.Abstractions;

public interface IRefreshSessionManager
{
    Task<Result<RefreshSession, Error>> GetByRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    void Delete(RefreshSession refreshSession);
}
