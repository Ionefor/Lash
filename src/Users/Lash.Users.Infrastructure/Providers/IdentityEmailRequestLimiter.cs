using Lash.Users.Application.Abstractions;
using Lash.Users.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Lash.Users.Infrastructure.Providers;

public sealed class IdentityEmailRequestLimiter(
    IOptions<IdentityEmailRateLimitOptions> options,
    TimeProvider timeProvider,
    IIdentityEmailRequestStore store) : IIdentityEmailRequestLimiter
{
    public async Task<bool> TryAcquireAsync(
        string email,
        IdentityEmailOperation operation,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var rule = GetRule(options.Value, operation);
        return await store.TryAcquireAsync(
            IdentityEmailRequestHasher.Hash(email),
            operation.ToString(),
            timeProvider.GetUtcNow(),
            rule.Limit,
            rule.Window,
            cancellationToken);
    }

    private static IdentityEmailRateLimitRule GetRule(
        IdentityEmailRateLimitOptions options,
        IdentityEmailOperation operation) =>
        operation switch
        {
            IdentityEmailOperation.EmailConfirmation => new(
                options.EmailConfirmationLimit,
                TimeSpan.FromMinutes(options.EmailConfirmationWindowMinutes)),
            IdentityEmailOperation.PasswordReset => new(
                options.PasswordResetLimit,
                TimeSpan.FromMinutes(options.PasswordResetWindowMinutes)),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
}

internal sealed record IdentityEmailRateLimitRule(int Limit, TimeSpan Window);
