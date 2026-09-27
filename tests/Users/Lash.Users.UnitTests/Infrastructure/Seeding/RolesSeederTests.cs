using Lash.Users.Application.Constants;
using Lash.Users.Infrastructure.Identity;
using Lash.Users.Infrastructure.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lash.Users.UnitTests.Infrastructure.Seeding;

public sealed class RolesSeederTests
{
    [Fact]
    public async Task SeedAsync_WhenRolesAreMissing_CreatesAllRequiredRoles()
    {
        var roleManager = CreateRoleManager();
        roleManager.Setup(manager => manager.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        roleManager.Setup(manager => manager.CreateAsync(It.IsAny<IdentityRoleEntity>()))
            .ReturnsAsync(IdentityResult.Success);

        var seeder = new RolesSeeder(roleManager.Object);

        await seeder.SeedAsync();

        roleManager.Verify(manager => manager.CreateAsync(It.Is<IdentityRoleEntity>(role => role.Name == AccountRoleNames.Client)), Times.Once);
        roleManager.Verify(manager => manager.CreateAsync(It.Is<IdentityRoleEntity>(role => role.Name == AccountRoleNames.Master)), Times.Once);
        roleManager.Verify(manager => manager.CreateAsync(It.Is<IdentityRoleEntity>(role => role.Name == AccountRoleNames.Admin)), Times.Once);
    }

    [Fact]
    public async Task SeedAsync_WhenRolesAlreadyExist_DoesNotCreateRoles()
    {
        var roleManager = CreateRoleManager();
        roleManager.Setup(manager => manager.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var seeder = new RolesSeeder(roleManager.Object);

        await seeder.SeedAsync();

        roleManager.Verify(manager => manager.CreateAsync(It.IsAny<IdentityRoleEntity>()), Times.Never);
    }

    private static Mock<RoleManager<IdentityRoleEntity>> CreateRoleManager()
    {
        return new Mock<RoleManager<IdentityRoleEntity>>(
            Mock.Of<IRoleStore<IdentityRoleEntity>>(),
            Enumerable.Empty<IRoleValidator<IdentityRoleEntity>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<ILogger<RoleManager<IdentityRoleEntity>>>());
    }
}
