using Lash.Users.Application.Features.Commands.RegisterMaster;
using Lash.Users.Application.Abstractions;
using Lash.Users.Domain;
using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Lash.Users.UnitTests.Application.Features.Commands.RegisterMaster;

public sealed class RegisterMasterHandlerTests
{
    [Fact]
    public async Task Handle_WhenCommandIsValid_CreatesUserWithMasterRole()
    {
        var role = CreateRole(RoleNames.Master);
        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager();
        User? createdUser = null;

        roleManager.Setup(manager => manager.FindByNameAsync(RoleNames.Master))
            .ReturnsAsync(role);
        userManager.Setup(manager => manager.CreateAsync(It.IsAny<User>(), "Password1!"))
            .Callback<User, string>((user, _) => createdUser = user)
            .ReturnsAsync(IdentityResult.Success);

        var handler = new RegisterMasterHandler(
            new RegisterMasterCommandValidator(),
            userManager.Object,
            roleManager.Object,
            CreateEmailConfirmationSender().Object);

        var result = await handler.Handle(new RegisterMasterCommand(
            "master@example.com",
            "Password1!",
            "Password1!"));

        Assert.True(result.IsSuccess);
        Assert.NotNull(createdUser);
        Assert.Equal(createdUser.Id, result.Value);
        Assert.Equal("master@example.com", createdUser.Email);
        Assert.Equal("master@example.com", createdUser.UserName);
        Assert.Contains(role, createdUser.Roles);
    }

    private static Mock<UserManager<User>> CreateUserManager()
    {
        return new Mock<UserManager<User>>(
            new Mock<IUserStore<User>>().Object,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<User>(),
            Array.Empty<IUserValidator<User>>(),
            Array.Empty<IPasswordValidator<User>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            NullLogger<UserManager<User>>.Instance);
    }

    private static Mock<RoleManager<Role>> CreateRoleManager()
    {
        return new Mock<RoleManager<Role>>(
            new Mock<IRoleStore<Role>>().Object,
            Array.Empty<IRoleValidator<Role>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            NullLogger<RoleManager<Role>>.Instance);
    }

    private static Mock<IEmailConfirmationSender> CreateEmailConfirmationSender()
    {
        var sender = new Mock<IEmailConfirmationSender>();
        sender.Setup(item => item.SendAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UnitResult.Success<Error>());
        return sender;
    }

    private static Role CreateRole(string name)
    {
        var result = Role.Create(name);

        Assert.True(result.IsSuccess);
        return result.Value;
    }
}
