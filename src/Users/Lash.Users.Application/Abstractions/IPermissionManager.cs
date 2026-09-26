namespace Lash.Users.Application.Abstractions;

public interface IPermissionManager
{
    Task<IReadOnlySet<string>> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
