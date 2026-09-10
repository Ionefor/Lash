using ErrorsFlow.Errors;
using Lash.Users.Domain;

namespace Lash.Users.UnitTests.Domain;

public sealed class UserTests
{
    [Fact]
    public void RegisterClient_WhenClientRoleProvided_ReturnsUserWithEmailAsUserName()
    {
        var role = CreateRole(RoleNames.Client);

        var result = User.RegisterClient("client@example.com", role);

        Assert.True(result.IsSuccess);
        Assert.Equal("client@example.com", result.Value.UserName);
        Assert.Equal("client@example.com", result.Value.Email);
        Assert.Contains(role, result.Value.Roles);
    }

    [Fact]
    public void RegisterClient_WhenEmailMissing_ReturnsRequiredValueError()
    {
        var role = CreateRole(RoleNames.Client);

        var result = User.RegisterClient(" ", role);

        Assert.True(result.IsFailure);
        Assert.Equal(GeneralErrorCodes.ValueIsRequired, result.Error.Code);
        Assert.Equal(nameof(User.Email), result.Error.Target);
    }

    [Fact]
    public void RegisterClient_WhenRoleDoesNotMatch_ReturnsInvalidRoleError()
    {
        var role = CreateRole(RoleNames.Master);

        var result = User.RegisterClient("client@example.com", role);

        Assert.True(result.IsFailure);
        Assert.Equal(AuthErrorCodes.RoleInvalid, result.Error.Code);
        Assert.Equal(RoleNames.Client, result.Error.Metadata["expectedRole"]);
    }

    private static Role CreateRole(string name)
    {
        var result = Role.Create(name);

        Assert.True(result.IsSuccess);
        return result.Value;
    }
}
