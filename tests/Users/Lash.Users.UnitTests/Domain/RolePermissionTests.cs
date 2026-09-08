using ErrorsFlow.Errors;
using Lash.Users.Domain;

namespace Lash.Users.UnitTests.Domain;

public sealed class RolePermissionTests
{
    [Fact]
    public void Create_WhenIdentifiersProvided_ReturnsRolePermission()
    {
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var result = RolePermission.Create(roleId, permissionId);

        Assert.True(result.IsSuccess);
        Assert.Equal(roleId, result.Value.RoleId);
        Assert.Equal(permissionId, result.Value.PermissionId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_WhenIdentifierIsEmpty_ReturnsInvalidValueError(bool isRoleIdEmpty)
    {
        var roleId = isRoleIdEmpty ? Guid.Empty : Guid.NewGuid();
        var permissionId = isRoleIdEmpty ? Guid.NewGuid() : Guid.Empty;

        var result = RolePermission.Create(roleId, permissionId);

        Assert.True(result.IsFailure);
        Assert.Equal(GeneralErrorCodes.ValueIsInvalid, result.Error.Code);
        Assert.Equal(
            isRoleIdEmpty ? nameof(RolePermission.RoleId) : nameof(RolePermission.PermissionId),
            result.Error.Target);
    }
}
