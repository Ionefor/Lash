using Lash.Users.Domain;
using Lash.Users.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Lash.Users.Infrastructure.Seeding;

public sealed class AdminSeeder(
    IOptions<AdminOptions> options,
    RoleManager<Role> roleManager,
    UserManager<User> userManager)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var adminOptions = options.Value;
        var emailIsMissing = string.IsNullOrWhiteSpace(adminOptions.Email);
        var passwordIsMissing = string.IsNullOrWhiteSpace(adminOptions.Password);
        if (emailIsMissing && passwordIsMissing)
        {
            return;
        }

        if (emailIsMissing || passwordIsMissing)
        {
            throw new InvalidOperationException(
                "Both Admin:Email and Admin:Password must be configured to seed the admin user.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (await userManager.FindByEmailAsync(adminOptions.Email) is not null)
        {
            return;
        }

        var adminRole = await roleManager.FindByNameAsync(RoleNames.Admin)
            ?? throw new InvalidOperationException("Admin role must be seeded before the admin user.");
        var adminResult = User.CreateAdmin(adminOptions.Email, adminRole);
        if (adminResult.IsFailure)
        {
            throw new InvalidOperationException(adminResult.Error.Message);
        }

        var creationResult = await userManager.CreateAsync(adminResult.Value, adminOptions.Password);
        if (!creationResult.Succeeded)
        {
            var errors = string.Join(", ", creationResult.Errors.Select(error => error.Code));
            throw new InvalidOperationException($"Could not seed admin user: {errors}");
        }
    }
}
