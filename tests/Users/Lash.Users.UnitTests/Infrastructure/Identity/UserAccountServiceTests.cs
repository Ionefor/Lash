using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Errors;
using Lash.Users.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lash.Users.UnitTests.Infrastructure.Identity;

public sealed class UserAccountServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenEmailAlreadyExists_ReturnsConflictForLowerCamelCaseTarget()
    {
        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.CreateAsync(It.IsAny<IdentityUserEntity>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = nameof(IdentityErrorDescriber.DuplicateEmail) }));
        var service = CreateService(userManager.Object);

        var result = await service.CreateAsync("user@example.com", "Password1!", "client");

        Assert.True(result.IsFailure);
        Assert.Equal(GeneralErrorCodes.ValueAlreadyExists, result.Error.Code);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal("email", result.Error.Target);
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

    private static UserAccountService CreateService(UserManager<IdentityUserEntity> userManager) =>
        new(userManager, CreateRoleManager().Object);

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
