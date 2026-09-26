using Lash.Users.Domain;
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
        roleManager.Setup(manager => manager.CreateAsync(It.IsAny<Role>()))
            .ReturnsAsync(IdentityResult.Success);

        var seeder = new RolesSeeder(roleManager.Object);

        await seeder.SeedAsync();

        roleManager.Verify(manager => manager.CreateAsync(It.Is<Role>(role => role.Name == RoleNames.Client)), Times.Once);
        roleManager.Verify(manager => manager.CreateAsync(It.Is<Role>(role => role.Name == RoleNames.Master)), Times.Once);
        roleManager.Verify(manager => manager.CreateAsync(It.Is<Role>(role => role.Name == RoleNames.Admin)), Times.Once);
    }

    [Fact]
    public async Task SeedAsync_WhenRolesAlreadyExist_DoesNotCreateRoles()
    {
        var roleManager = CreateRoleManager();
        roleManager.Setup(manager => manager.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var seeder = new RolesSeeder(roleManager.Object);

        await seeder.SeedAsync();

        roleManager.Verify(manager => manager.CreateAsync(It.IsAny<Role>()), Times.Never);
    }

    private static Mock<RoleManager<Role>> CreateRoleManager()
    {
        return new Mock<RoleManager<Role>>(
            Mock.Of<IRoleStore<Role>>(),
            Enumerable.Empty<IRoleValidator<Role>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<ILogger<RoleManager<Role>>>());
    }
}
