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
    public async Task SynchronizeAsync_WhenPermissionIsUnknown_ThrowsWithoutAddingRolePermission()
    {
        await using var context = CreateContext();
        var manager = new RolePermissionManager(context);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            manager.SynchronizeAsync(Guid.NewGuid(), ["users.read"]));

        Assert.Equal("Permissions must be seeded before roles: users.read.", exception.Message);
        Assert.Empty(context.RolePermissions);
    }

    [Fact]
    public async Task SynchronizeAsync_WhenRoleHasPermissionRemovedFromConfiguration_RevokesOnlyThatPermission()
    {
        await using var context = CreateContext();
        var role = IdentityRoleEntity.Create(AccountRoleNames.Client);
        var readPermission = IdentityPermission.Create("users.read");
        var managePermission = IdentityPermission.Create("users.manage");
        context.AddRange(role, readPermission, managePermission);
        await context.SaveChangesAsync();
        context.RolePermissions.AddRange(
            IdentityRolePermission.Create(role.Id, readPermission.Id),
            IdentityRolePermission.Create(role.Id, managePermission.Id));
        await context.SaveChangesAsync();
        var manager = new RolePermissionManager(context);

        await manager.SynchronizeAsync(role.Id, ["users.read"]);
        await context.SaveChangesAsync();

        var assignedCodes = await context.RolePermissions
            .Where(item => item.RoleId == role.Id)
            .Join(
                context.Permissions,
                rolePermission => rolePermission.PermissionId,
                permission => permission.Id,
                (_, permission) => permission.Code)
            .ToArrayAsync();

        Assert.Equal(["users.read"], assignedCodes);
        Assert.NotNull(await context.Permissions.SingleOrDefaultAsync(permission => permission.Id == managePermission.Id));
    }

    [Fact]
    public async Task SynchronizeAsync_WhenRoleHasNoConfiguredPermissions_RemovesAllRolePermissions()
    {
        await using var context = CreateContext();
        var role = IdentityRoleEntity.Create(AccountRoleNames.Client);
        var permission = IdentityPermission.Create("users.read");
        context.AddRange(role, permission);
        await context.SaveChangesAsync();
        context.RolePermissions.Add(IdentityRolePermission.Create(role.Id, permission.Id));
        await context.SaveChangesAsync();
        var manager = new RolePermissionManager(context);

        await manager.SynchronizeAsync(role.Id, []);
        await context.SaveChangesAsync();

        Assert.Empty(await context.RolePermissions.Where(item => item.RoleId == role.Id).ToArrayAsync());
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
