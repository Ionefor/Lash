namespace Lash.Users.Infrastructure.Authorization;

public interface IPermissionManager
{
    Task<IReadOnlySet<string>> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
