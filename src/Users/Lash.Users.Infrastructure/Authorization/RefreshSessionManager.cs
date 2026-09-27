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

    public async Task<bool> TryRevokeAsync(
        Guid sessionId,
        DateTimeOffset revokedAt,
        CancellationToken cancellationToken = default)
    {
        var updated = await dbContext.RefreshSessions
            .Where(session =>
                session.Id == sessionId &&
                session.RevokedAt == null &&
                session.ExpiresAt > revokedAt)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(session => session.RevokedAt, revokedAt),
                cancellationToken);

        return updated == 1;
    }

    public async Task RevokeAllForUserAsync(
        Guid userId,
        DateTimeOffset revokedAt,
        CancellationToken cancellationToken = default)
    {
        await dbContext.RefreshSessions
            .Where(session => session.UserId == userId && session.RevokedAt == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(session => session.RevokedAt, revokedAt),
                cancellationToken);
    }
}
