namespace Lash.Users.Application.Abstractions;

public interface IIdentityEmailCodeAttemptLimiter
{
    Task<bool> TryAcquireAsync(
        string email,
        IdentityEmailOperation operation,
        CancellationToken cancellationToken = default);

    Task CompleteAsync(
        string email,
        IdentityEmailOperation operation,
        CancellationToken cancellationToken = default);
}
