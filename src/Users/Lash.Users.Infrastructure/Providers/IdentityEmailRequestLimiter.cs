using System.Security.Cryptography;
using System.Text;
using Lash.Users.Application.Abstractions;
using Lash.Users.Domain;
using Lash.Users.Infrastructure.DbContexts;
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
        var requestCount = await dbContext.IdentityEmailRequests.CountAsync(
            request => request.EmailHash == emailHash &&
                       request.Operation == operationName &&
                       request.RequestedAt >= now.Subtract(window),
            cancellationToken);
        if (requestCount >= limit)
            return false;

        dbContext.IdentityEmailRequests.Add(IdentityEmailRequest.Create(emailHash, operationName, now));
        return true;
    }

    private static string HashEmail(string email) => Convert.ToHexString(SHA256.HashData(
        Encoding.UTF8.GetBytes(email.Trim().ToUpperInvariant())));
}
