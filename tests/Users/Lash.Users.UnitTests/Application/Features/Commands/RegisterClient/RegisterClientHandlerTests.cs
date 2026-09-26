using Lash.Users.Application.Features.Commands.RegisterClient;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Errors;
using Lash.Users.Domain;
using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Lash.Users.UnitTests.Application.Features.Commands.RegisterClient;

public sealed class RegisterClientHandlerTests
{
    [Fact]
    public async Task Handle_WhenCommandIsValid_CreatesUserWithClientRole()
    {
        var role = CreateRole(RoleNames.Client);
        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager();
        var emailConfirmationSender = CreateEmailConfirmationSender();
        User? createdUser = null;

        roleManager.Setup(manager => manager.FindByNameAsync(RoleNames.Client))
            .ReturnsAsync(role);
        userManager.Setup(manager => manager.CreateAsync(It.IsAny<User>(), "Password1!"))
            .Callback<User, string>((user, _) => createdUser = user)
            .ReturnsAsync(IdentityResult.Success);

        var handler = new RegisterClientHandler(
            new RegisterClientCommandValidator(),
            userManager.Object,
            roleManager.Object,
            emailConfirmationSender.Object);

        var result = await handler.Handle(new RegisterClientCommand(
            "client@example.com",
            "Password1!",
            "Password1!"));

        Assert.True(result.IsSuccess);
        Assert.NotNull(createdUser);
        Assert.Equal(createdUser.Id, result.Value);
        Assert.Equal("client@example.com", createdUser.Email);
        Assert.Equal("client@example.com", createdUser.UserName);
        Assert.Contains(role, createdUser.Roles);
        emailConfirmationSender.Verify(
            sender => sender.SendAsync(createdUser, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ReturnsConflictErrorWithoutIdentityDescription()
    {
        var role = CreateRole(RoleNames.Client);
        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager();

        roleManager.Setup(manager => manager.FindByNameAsync(RoleNames.Client))
            .ReturnsAsync(role);
        userManager.Setup(manager => manager.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = nameof(IdentityErrorDescriber.DuplicateUserName),
                Description = "Username 'client@example.com' is already taken."
            }));

        var handler = new RegisterClientHandler(
            new RegisterClientCommandValidator(),
            userManager.Object,
            roleManager.Object,
            CreateEmailConfirmationSender().Object);

        var result = await handler.Handle(new RegisterClientCommand(
            "client@example.com",
            "Password1!",
            "Password1!"));

        Assert.True(result.IsFailure);
        Assert.Equal(GeneralErrorCodes.ValueAlreadyExists, result.Error[0].Code);
        Assert.Equal(nameof(RegisterClientCommand.Email), result.Error[0].Target);
        Assert.DoesNotContain("client@example.com", result.Error[0].Message);
    }

    [Fact]
    public async Task Handle_WhenClientRoleIsNotSeeded_ReturnsModuleConfigurationError()
    {
        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager();
        roleManager.Setup(manager => manager.FindByNameAsync(RoleNames.Client))
            .ReturnsAsync((Role?)null);
        var handler = new RegisterClientHandler(
            new RegisterClientCommandValidator(),
            userManager.Object,
            roleManager.Object,
            CreateEmailConfirmationSender().Object);

        var result = await handler.Handle(new RegisterClientCommand(
            "client@example.com",
            "Password1!",
            "Password1!"));

        Assert.True(result.IsFailure);
        Assert.Equal(UsersApplicationErrorCodes.RequiredRoleNotConfigured, result.Error[0].Code);
        Assert.Equal("role", result.Error[0].Target);
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
