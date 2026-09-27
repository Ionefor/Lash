using Lash.Users.Application.Constants;
using Lash.Users.Infrastructure.Identity;
using Lash.Users.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Infrastructure.Seeding;

public sealed class AdminSeeder(
    IOptions<AdminOptions> options,
    RoleManager<IdentityRoleEntity> roleManager,
    UserManager<IdentityUserEntity> userManager,
    IUnitOfWork unitOfWork) : ISeeder
{
    public int Order => 300;

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
        var existingUser = await userManager.FindByEmailAsync(adminOptions.Email);
        if (existingUser is not null)
        {
            if (await userManager.IsInRoleAsync(existingUser, AccountRoleNames.Admin))
            {
                return;
            }

            throw new InvalidOperationException(
                "Admin user cannot be seeded because the configured account already exists without the admin role.");
        }

        var adminRole = await roleManager.FindByNameAsync(AccountRoleNames.Admin)
            ?? throw new InvalidOperationException("Admin role must be seeded before the admin user.");

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var user = IdentityUserEntity.Create(adminOptions.Email);
        var creationResult = await userManager.CreateAsync(user, adminOptions.Password);
        if (!creationResult.Succeeded)
        {
            var errors = string.Join(", ", creationResult.Errors.Select(error => error.Code));
            throw new InvalidOperationException($"Could not seed admin user: {errors}");
        }

        var roleResult = await userManager.AddToRoleAsync(user, adminRole.Name!);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(error => error.Code));
            throw new InvalidOperationException($"Could not assign admin role: {errors}");
        }

        await transaction.CommitAsync(cancellationToken);
    }
}
