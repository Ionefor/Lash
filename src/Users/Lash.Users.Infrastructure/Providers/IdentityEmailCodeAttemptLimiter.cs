using Lash.Users.Application.Abstractions;
using Lash.Users.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Lash.Users.Infrastructure.Providers;

public sealed class IdentityEmailCodeAttemptLimiter(
    IOptions<IdentityEmailRateLimitOptions> options,
    TimeProvider timeProvider,
    IIdentityEmailRequestStore store) : IIdentityEmailCodeAttemptLimiter
{
    public Task<bool> TryAcquireAsync(string email, IdentityEmailOperation operation, CancellationToken cancellationToken = default) =>
        store.TryAcquireCodeAttemptAsync(
            IdentityEmailRequestHasher.Hash(email),
            operation.ToString(),
            timeProvider.GetUtcNow(),
            options.Value.CodeVerificationAttemptLimit,
            cancellationToken);

    public Task CompleteAsync(string email, IdentityEmailOperation operation, CancellationToken cancellationToken = default) =>
        store.CompleteCodeChallengeAsync(
            IdentityEmailRequestHasher.Hash(email),
            operation.ToString(),
            timeProvider.GetUtcNow(),
            cancellationToken);
}
