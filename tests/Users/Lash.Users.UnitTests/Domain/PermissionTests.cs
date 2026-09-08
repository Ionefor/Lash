using ErrorsFlow.Errors;
using Lash.Users.Domain;

namespace Lash.Users.UnitTests.Domain;

public sealed class PermissionTests
{
    [Fact]
    public void Create_WhenCodeProvided_ReturnsPermission()
    {
        var result = Permission.Create("users.read");

        Assert.True(result.IsSuccess);
        Assert.Equal("users.read", result.Value.Code);
    }

    [Fact]
    public void Create_WhenCodeMissing_ReturnsRequiredValueError()
    {
        var result = Permission.Create(" ");

        Assert.True(result.IsFailure);
        Assert.Equal(GeneralErrorCodes.ValueIsRequired, result.Error.Code);
        Assert.Equal(nameof(Permission.Code), result.Error.Target);
    }
}
