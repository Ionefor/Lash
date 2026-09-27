using Lash.Users.Application.Abstractions;
using Lash.Users.Infrastructure.Authorization;
using Lash.Users.Infrastructure.Identity;
using Lash.Users.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Lash.Users.Infrastructure.Seeding;

public sealed class PermissionManager(UsersDbContext dbContext) : IPermissionManager
{
    public async Task AddMissingAsync(
        IEnumerable<string> permissionCodes,
        CancellationToken cancellationToken = default)
    {
        var codes = permissionCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var existingCodes = await dbContext.Permissions
            .Where(permission => codes.Contains(permission.Code))
            .Select(permission => permission.Code)
            .ToHashSetAsync(cancellationToken);

        var permissions = codes
            .Where(code => !existingCodes.Contains(code))
            .Select(IdentityPermission.Create);

        await dbContext.Permissions.AddRangeAsync(permissions, cancellationToken);
    }

    public async Task<IReadOnlySet<string>> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var permissions = await dbContext.Users
            .Where(user => user.Id == userId)
            .SelectMany(user => user.Roles)
            .SelectMany(role => role.RolePermissions)
            .Select(rolePermission => rolePermission.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        return permissions.ToHashSet(StringComparer.Ordinal);
    }
}
