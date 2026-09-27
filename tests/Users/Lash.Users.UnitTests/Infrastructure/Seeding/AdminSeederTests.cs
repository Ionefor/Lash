using Lash.Users.Application.Constants;
using Lash.Users.Infrastructure.Identity;
using Lash.Users.Infrastructure.Options;
using Lash.Users.Infrastructure.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.UnitTests.Infrastructure.Seeding;

public sealed class AdminSeederTests
{
    [Fact]
    public async Task SeedAsync_WhenAdminUserExistsWithoutRole_AddsAdminRole()
    {
        var user = IdentityUserEntity.Create("admin@example.com");
        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByEmailAsync("admin@example.com")).ReturnsAsync(user);
        userManager.Setup(manager => manager.IsInRoleAsync(user, AccountRoleNames.Admin)).ReturnsAsync(false);
        userManager.Setup(manager => manager.AddToRoleAsync(user, AccountRoleNames.Admin)).ReturnsAsync(IdentityResult.Success);
        var unitOfWork = new Mock<IUnitOfWork>();
        var seeder = CreateSeeder(userManager.Object, unitOfWork.Object);

        await seeder.SeedAsync();

        userManager.Verify(manager => manager.AddToRoleAsync(user, AccountRoleNames.Admin), Times.Once);
        userManager.Verify(manager => manager.CreateAsync(It.IsAny<IdentityUserEntity>(), It.IsAny<string>()), Times.Never);
        unitOfWork.Verify(manager => manager.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SeedAsync_WhenCreatingAdmin_AssignsRoleAndCommitsTransaction()
    {
        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByEmailAsync("admin@example.com")).ReturnsAsync((IdentityUserEntity?)null);
        userManager.Setup(manager => manager.CreateAsync(It.IsAny<IdentityUserEntity>(), "StrongPassword1!"))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(manager => manager.AddToRoleAsync(It.IsAny<IdentityUserEntity>(), AccountRoleNames.Admin))
            .ReturnsAsync(IdentityResult.Success);
        var roleManager = CreateRoleManager();
        roleManager.Setup(manager => manager.FindByNameAsync(AccountRoleNames.Admin))
            .ReturnsAsync(IdentityRoleEntity.Create(AccountRoleNames.Admin));
        var transaction = new Mock<ITransaction>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(manager => manager.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction.Object);
        var seeder = CreateSeeder(userManager.Object, unitOfWork.Object, roleManager.Object);

        await seeder.SeedAsync();

        userManager.Verify(manager => manager.AddToRoleAsync(It.IsAny<IdentityUserEntity>(), AccountRoleNames.Admin), Times.Once);
        transaction.Verify(item => item.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static AdminSeeder CreateSeeder(
        UserManager<IdentityUserEntity> userManager,
        IUnitOfWork unitOfWork,
        RoleManager<IdentityRoleEntity>? roleManager = null) =>
        new(
            Options.Create(new AdminOptions { Email = "admin@example.com", Password = "StrongPassword1!" }),
            roleManager ?? CreateRoleManager().Object,
            userManager,
            unitOfWork);

    private static Mock<UserManager<IdentityUserEntity>> CreateUserManager() =>
        new(
            Mock.Of<IUserStore<IdentityUserEntity>>(),
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            Mock.Of<ILogger<UserManager<IdentityUserEntity>>>());

    private static Mock<RoleManager<IdentityRoleEntity>> CreateRoleManager() =>
        new(
            Mock.Of<IRoleStore<IdentityRoleEntity>>(),
            Enumerable.Empty<IRoleValidator<IdentityRoleEntity>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<ILogger<RoleManager<IdentityRoleEntity>>>());
}
