using Lash.Users.Domain;
using Lash.Users.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Lash.Users.Infrastructure.Seeding;

public sealed class RolePermissionManager(UsersDbContext dbContext)
{
    public async Task AddMissingAsync(
        Guid roleId,
        IEnumerable<string> permissionCodes,
        CancellationToken cancellationToken = default)
    {
        var codes = permissionCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var permissions = await dbContext.Permissions
            .Where(permission => codes.Contains(permission.Code))
            .ToListAsync(cancellationToken);

        if (permissions.Count != codes.Length)
        {
            var foundCodes = permissions.Select(permission => permission.Code).ToHashSet();
            var missingCodes = codes.Where(code => !foundCodes.Contains(code));
            throw new InvalidOperationException(
                $"Permissions must be seeded before roles: {string.Join(", ", missingCodes)}.");
        }

        var permissionIds = permissions.Select(permission => permission.Id).ToArray();
        var existingIds = await dbContext.RolePermissions
            .Where(item => item.RoleId == roleId && permissionIds.Contains(item.PermissionId))
            .Select(item => item.PermissionId)
            .ToHashSetAsync(cancellationToken);

        var rolePermissions = permissions
            .Where(permission => !existingIds.Contains(permission.Id))
            .Select(permission => RolePermission.Create(roleId, permission.Id).Value);

        await dbContext.RolePermissions.AddRangeAsync(rolePermissions, cancellationToken);
    }
}
