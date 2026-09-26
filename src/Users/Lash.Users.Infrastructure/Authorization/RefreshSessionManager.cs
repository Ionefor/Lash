using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Domain;
using Lash.Users.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Lash.Users.Infrastructure.Authorization;

public sealed class RefreshSessionManager(UsersDbContext dbContext) : IRefreshSessionManager
{
    public async Task<Result<RefreshSession, Error>> GetByRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result.Failure<RefreshSession, Error>(AuthErrors.RefreshTokenInvalid());
        }

        var tokenHash = RefreshTokenHasher.Hash(refreshToken);
        var session = await dbContext.RefreshSessions
            .Include(item => item.User)
            .SingleOrDefaultAsync(item => item.TokenHash == tokenHash, cancellationToken);

        if (session is null)
        {
            return Result.Failure<RefreshSession, Error>(AuthErrors.RefreshTokenInvalid());
        }

        var activeResult = session.EnsureActive(DateTimeOffset.UtcNow);
        return activeResult.IsFailure
            ? Result.Failure<RefreshSession, Error>(activeResult.Error)
            : Result.Success<RefreshSession, Error>(session);
    }

    public void Delete(RefreshSession refreshSession) =>
        dbContext.RefreshSessions.Remove(refreshSession);
}
