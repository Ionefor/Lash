using Lash.Users.Infrastructure.Identity;
using Lash.Users.Infrastructure.DbContexts;
using Lash.Users.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Lash.Users.Infrastructure.Seeding;

public sealed class PermissionsSeeder(
    IOptions<RolePermissionOptions> options,
    RoleManager<IdentityRoleEntity> roleManager,
    PermissionManager permissionManager,
    RolePermissionManager rolePermissionManager,
    UsersDbContext dbContext) : ISeeder
{
    public int Order => 200;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var seedOptions = options.Value;
        var permissionCodes = seedOptions.Permissions.Values
            .SelectMany(codes => codes)
            .Concat(seedOptions.Roles.Values.SelectMany(codes => codes))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        await permissionManager.AddMissingAsync(permissionCodes, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var (roleName, rolePermissionCodes) in seedOptions.Roles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var role = await roleManager.FindByNameAsync(roleName)
                ?? throw new InvalidOperationException(
                    $"Role '{roleName}' must be seeded before its permissions.");

            await rolePermissionManager.SynchronizeAsync(
                role.Id,
                rolePermissionCodes,
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
