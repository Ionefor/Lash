namespace Lash.Users.Infrastructure.Providers;

public interface IIdentityEmailRequestStore
{
    Task<bool> TryAcquireAsync(
        string emailHash,
        string operation,
        DateTimeOffset requestedAt,
        int limit,
        TimeSpan window,
        CancellationToken cancellationToken = default);

    Task<bool> TryAcquireCodeAttemptAsync(
        string emailHash,
        string operation,
        DateTimeOffset attemptedAt,
        int limit,
        CancellationToken cancellationToken = default);

    Task CompleteCodeChallengeAsync(
        string emailHash,
        string operation,
        DateTimeOffset completedAt,
        CancellationToken cancellationToken = default);
}
