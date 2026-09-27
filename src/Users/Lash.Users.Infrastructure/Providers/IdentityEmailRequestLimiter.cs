using System.Security.Cryptography;
using System.Text;
using Lash.Users.Application.Abstractions;
using Lash.Users.Infrastructure.DbContexts;
using Lash.Users.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lash.Users.Infrastructure.Providers;

public sealed class IdentityEmailRequestLimiter(UsersDbContext dbContext) : IIdentityEmailRequestLimiter
{
    public async Task<bool> TryAcquireAsync(
        string email,
        IdentityEmailOperation operation,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var (limit, window) = operation switch
        {
            IdentityEmailOperation.EmailConfirmation => (3, TimeSpan.FromMinutes(15)),
            IdentityEmailOperation.PasswordReset => (3, TimeSpan.FromHours(1)),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
        var now = DateTimeOffset.UtcNow;
        var operationName = operation.ToString();
        var emailHash = HashEmail(email);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var lockKey = $"{emailHash}:{operationName}";
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({lockKey}, 0))",
            cancellationToken);

        var requestCount = await dbContext.IdentityEmailRequests.CountAsync(
            request => request.EmailHash == emailHash &&
                       request.Operation == operationName &&
                       request.RequestedAt >= now.Subtract(window),
            cancellationToken);
        if (requestCount >= limit)
        {
            await transaction.CommitAsync(cancellationToken);
            return false;
        }

        dbContext.IdentityEmailRequests.Add(IdentityEmailRequest.Create(emailHash, operationName, now));
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private static string HashEmail(string email) => Convert.ToHexString(SHA256.HashData(
        Encoding.UTF8.GetBytes(email.Trim().ToUpperInvariant())));
}
