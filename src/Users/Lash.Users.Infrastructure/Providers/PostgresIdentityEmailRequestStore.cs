using Lash.Users.Infrastructure.DbContexts;
using Lash.Users.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Lash.Users.Infrastructure.Providers;

public sealed class PostgresIdentityEmailRequestStore(UsersDbContext dbContext) : IIdentityEmailRequestStore
{
    public async Task<bool> TryAcquireAsync(
        string emailHash,
        string operation,
        DateTimeOffset requestedAt,
        int limit,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        var ownsTransaction = dbContext.Database.CurrentTransaction is null;
        IDbContextTransaction? transaction = null;
        if (ownsTransaction)
        {
            transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        await using (transaction)
        {
            var lockKey = $"{emailHash}:{operation}";
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({lockKey}, 0))",
                cancellationToken);

            var requestCount = await dbContext.IdentityEmailRequests.CountAsync(
                request => request.EmailHash == emailHash &&
                           request.Operation == operation &&
                           request.RequestedAt >= requestedAt.Subtract(window),
                cancellationToken);
            if (requestCount >= limit)
            {
                if (transaction is not null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                return false;
            }

            dbContext.IdentityEmailRequests.Add(IdentityEmailRequest.Create(emailHash, operation, requestedAt));
            if (transaction is not null)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }

            return true;
        }
    }

    public async Task<bool> TryAcquireCodeAttemptAsync(
        string emailHash,
        string operation,
        DateTimeOffset attemptedAt,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var ownsTransaction = dbContext.Database.CurrentTransaction is null;
        await using var transaction = ownsTransaction
            ? await dbContext.Database.BeginTransactionAsync(cancellationToken)
            : null;
        await AcquireLockAsync(emailHash, operation, cancellationToken);
        var challenge = await dbContext.IdentityEmailRequests
            .Where(request => request.EmailHash == emailHash && request.Operation == operation)
            .OrderByDescending(request => request.RequestedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (challenge is null || !challenge.TryRecordFailedAttempt(attemptedAt, limit))
        {
            if (transaction is not null)
                await transaction.CommitAsync(cancellationToken);

            return false;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        if (transaction is not null)
            await transaction.CommitAsync(cancellationToken);

        return true;
    }

    public async Task CompleteCodeChallengeAsync(
        string emailHash,
        string operation,
        DateTimeOffset completedAt,
        CancellationToken cancellationToken = default)
    {
        var ownsTransaction = dbContext.Database.CurrentTransaction is null;
        await using var transaction = ownsTransaction
            ? await dbContext.Database.BeginTransactionAsync(cancellationToken)
            : null;
        await AcquireLockAsync(emailHash, operation, cancellationToken);
        var challenge = await dbContext.IdentityEmailRequests
            .Where(request => request.EmailHash == emailHash && request.Operation == operation)
            .OrderByDescending(request => request.RequestedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (challenge is not null)
        {
            challenge.Complete(completedAt);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (transaction is not null)
            await transaction.CommitAsync(cancellationToken);
    }

    private async Task AcquireLockAsync(string emailHash, string operation, CancellationToken cancellationToken)
    {
        var lockKey = $"{emailHash}:{operation}";
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({lockKey}, 0))",
            cancellationToken);
    }
}
