using Lash.Users.Application.Constants;
using Lash.Users.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Lash.Users.Infrastructure.Seeding;

public sealed class RolesSeeder(RoleManager<IdentityRoleEntity> roleManager) : ISeeder
{
    public int Order => 100;

    private static readonly string[] RoleNamesToSeed =
    [
        AccountRoleNames.Client,
        AccountRoleNames.Master,
        AccountRoleNames.Admin
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var roleName in RoleNamesToSeed)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var role = IdentityRoleEntity.Create(roleName);
            var result = await roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(error => error.Code));
                throw new InvalidOperationException($"Could not seed role '{roleName}': {errors}.");
            }
        }
    }
}
