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
}
