namespace Lash.Users.Application.Abstractions;

public interface IUserSessionLock
{
    Task<bool> TryAcquireAsync(Guid userId, CancellationToken cancellationToken = default);
}
