using Lash.Users.Domain;
using Microsoft.AspNetCore.Identity;

namespace Lash.Users.Infrastructure.Seeding;

public sealed class RolesSeeder(RoleManager<Role> roleManager)
{
    private static readonly string[] RoleNamesToSeed =
    [
        RoleNames.Client,
        RoleNames.Master,
        RoleNames.Admin
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

            var role = Role.Create(roleName).Value;
            var result = await roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(error => error.Code));
                throw new InvalidOperationException($"Could not seed role '{roleName}': {errors}.");
            }
        }
    }
}
