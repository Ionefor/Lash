using Lash.Users.Application.Constants;
using Lash.Users.Infrastructure.DbContexts;
using Lash.Users.Infrastructure.Identity;
using Lash.Users.Infrastructure.Options;
using Lash.Users.Infrastructure.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Lash.Users.UnitTests.Infrastructure.Seeding;

public sealed class PermissionsSeederTests
{
    [Fact]
    public async Task SeedAsync_WhenRunTwice_AddsPermissionsAndRolePermissionsOnce()
    {
        await using var context = CreateContext();
        var role = IdentityRoleEntity.Create(AccountRoleNames.Client);
        context.Roles.Add(role);
        await context.SaveChangesAsync();
        var roleManager = CreateRoleManager();
        roleManager.Setup(manager => manager.FindByNameAsync(AccountRoleNames.Client))
            .ReturnsAsync(role);
        var seeder = new PermissionsSeeder(
            Options.Create(new RolePermissionOptions
            {
                Permissions = new Dictionary<string, string[]>
                {
                    ["users"] = ["users.read", "users.manage"]
                },
                Roles = new Dictionary<string, string[]>
                {
                    [AccountRoleNames.Client] = ["users.read"]
                }
            }),
            roleManager.Object,
            new PermissionManager(context),
            new RolePermissionManager(context),
            context);

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        Assert.Equal(
            ["users.manage", "users.read"],
            await context.Permissions
                .Select(permission => permission.Code)
                .OrderBy(code => code)
                .ToArrayAsync());
        var rolePermission = Assert.Single(await context.RolePermissions.ToArrayAsync());
        Assert.Equal(role.Id, rolePermission.RoleId);
        Assert.Equal("users.read", await context.Permissions
            .Where(permission => permission.Id == rolePermission.PermissionId)
            .Select(permission => permission.Code)
            .SingleAsync());
    }

    [Fact]
    public async Task AddMissingAsync_WhenPermissionIsUnknown_ThrowsWithoutAddingRolePermission()
    {
        await using var context = CreateContext();
        var manager = new RolePermissionManager(context);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            manager.AddMissingAsync(Guid.NewGuid(), ["users.read"]));

        Assert.Equal("Permissions must be seeded before roles: users.read.", exception.Message);
        Assert.Empty(context.RolePermissions);
    }

    private static UsersDbContext CreateContext() => new(
        new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static Mock<RoleManager<IdentityRoleEntity>> CreateRoleManager() =>
        new(
            Mock.Of<IRoleStore<IdentityRoleEntity>>(),
            Enumerable.Empty<IRoleValidator<IdentityRoleEntity>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<ILogger<RoleManager<IdentityRoleEntity>>>());
}
