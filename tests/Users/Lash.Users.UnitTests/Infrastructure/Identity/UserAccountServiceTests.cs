using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Errors;
using Lash.Users.Infrastructure.DbContexts;
using Lash.Users.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;

namespace Lash.Users.UnitTests.Infrastructure.Identity;

public sealed class UserAccountServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenEmailAlreadyExists_ReturnsConflictForLowerCamelCaseTarget()
    {
        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager();
        roleManager.Setup(manager => manager.RoleExistsAsync("client")).ReturnsAsync(true);
        userManager.Setup(manager => manager.CreateAsync(It.IsAny<IdentityUserEntity>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = nameof(IdentityErrorDescriber.DuplicateEmail) }));
        var service = CreateService(userManager.Object, roleManager);

        var result = await service.CreateAsync("user@example.com", "Password1!", "client");

        Assert.True(result.IsFailure);
        Assert.Equal(GeneralErrorCodes.ValueAlreadyExists, result.Error.Code);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal("email", result.Error.Target);
    }

    [Fact]
    public async Task CreateAsync_WhenDatabaseReportsDuplicateEmail_ReturnsConflictForEmail()
    {
        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager();
        roleManager.Setup(manager => manager.RoleExistsAsync("client")).ReturnsAsync(true);
        userManager.Setup(manager => manager.CreateAsync(It.IsAny<IdentityUserEntity>(), It.IsAny<string>()))
            .ThrowsAsync(new DbUpdateException("Duplicate email.", CreateUniqueViolation("EmailIndex")));
        var service = CreateService(userManager.Object, roleManager);

        var result = await service.CreateAsync("user@example.com", "Password1!", "client");

        Assert.True(result.IsFailure);
        Assert.Equal(GeneralErrorCodes.ValueAlreadyExists, result.Error.Code);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal("email", result.Error.Target);
    }

    [Fact]
    public async Task CreateAsync_WhenRoleIsMissing_ReturnsFailureWithoutCreatingUser()
    {
        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager();
        roleManager.Setup(manager => manager.RoleExistsAsync("client")).ReturnsAsync(false);
        var service = CreateService(userManager.Object, roleManager);

        var result = await service.CreateAsync("user@example.com", "Password1!", "client");

        Assert.True(result.IsFailure);
        Assert.Equal(GeneralErrorCodes.Failed, result.Error.Code);
        userManager.Verify(manager => manager.CreateAsync(It.IsAny<IdentityUserEntity>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenRoleIsAssigned_ReturnsCreatedAccount()
    {
        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager();
        roleManager.Setup(manager => manager.RoleExistsAsync("client")).ReturnsAsync(true);
        userManager.Setup(manager => manager.CreateAsync(It.IsAny<IdentityUserEntity>(), "Password1!"))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(manager => manager.AddToRoleAsync(It.IsAny<IdentityUserEntity>(), "client"))
            .ReturnsAsync(IdentityResult.Success);
        var service = CreateService(userManager.Object, roleManager);

        var result = await service.CreateAsync("user@example.com", "Password1!", "client");

        Assert.True(result.IsSuccess);
        Assert.Equal("user@example.com", result.Value.Email);
        userManager.Verify(manager => manager.AddToRoleAsync(It.IsAny<IdentityUserEntity>(), "client"), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenRoleAssignmentFails_ReturnsFailure()
    {
        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager();
        roleManager.Setup(manager => manager.RoleExistsAsync("client")).ReturnsAsync(true);
        userManager.Setup(manager => manager.CreateAsync(It.IsAny<IdentityUserEntity>(), "Password1!"))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(manager => manager.AddToRoleAsync(It.IsAny<IdentityUserEntity>(), "client"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "StoreFailure" }));
        var service = CreateService(userManager.Object, roleManager);

        var result = await service.CreateAsync("user@example.com", "Password1!", "client");

        Assert.True(result.IsFailure);
        Assert.Equal(GeneralErrorCodes.Failed, result.Error.Code);
    }

    [Fact]
    public async Task FindByEmailAsync_WhenCancellationIsRequested_DoesNotCallIdentityStore()
    {
        var userManager = CreateUserManager();
        var service = CreateService(userManager.Object);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            service.FindByEmailAsync("user@example.com", cancellationSource.Token));

        userManager.Verify(manager => manager.FindByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmEmailAsync_WhenTokenIsInvalid_ReturnsConfirmationCodeValidationError()
    {
        var user = IdentityUserEntity.Create("user@example.com");
        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userManager.Setup(manager => manager.ConfirmEmailAsync(user, "code"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = nameof(IdentityErrorDescriber.InvalidToken) }));
        var service = CreateService(userManager.Object);

        var result = await service.ConfirmEmailAsync(user.Id, "code");

        Assert.True(result.IsFailure);
        Assert.Equal(UsersApplicationErrorCodes.EmailConfirmationCodeInvalid, result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("code", result.Error.Target);
    }

    [Fact]
    public async Task ConfirmEmailAsync_WhenIdentityOperationFails_ReturnsFailureError()
    {
        var user = IdentityUserEntity.Create("user@example.com");
        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userManager.Setup(manager => manager.ConfirmEmailAsync(user, "code"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "StoreFailure" }));
        var service = CreateService(userManager.Object);

        var result = await service.ConfirmEmailAsync(user.Id, "code");

        Assert.True(result.IsFailure);
        Assert.Equal(GeneralErrorCodes.Failed, result.Error.Code);
        Assert.Equal(ErrorType.Failure, result.Error.Type);
        Assert.Null(result.Error.Target);
    }

    private static UserAccountService CreateService(
        UserManager<IdentityUserEntity> userManager,
        Mock<RoleManager<IdentityRoleEntity>>? roleManager = null) =>
        new(userManager, (roleManager ?? CreateRoleManager()).Object, CreateDbContext());

    private static UsersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new UsersDbContext(options);
    }

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

    private static PostgresException CreateUniqueViolation(string constraintName) =>
        new(
            "Duplicate key value violates unique constraint.",
            "ERROR",
            "ERROR",
            PostgresErrorCodes.UniqueViolation,
            constraintName: constraintName);
}
