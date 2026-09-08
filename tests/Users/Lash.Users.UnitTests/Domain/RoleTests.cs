using ErrorsFlow.Errors;
using Lash.Users.Domain;

namespace Lash.Users.UnitTests.Domain;

public sealed class RoleTests
{
    [Fact]
    public void Create_WhenNameProvided_ReturnsRole()
    {
        var result = Role.Create(RoleNames.Master);

        Assert.True(result.IsSuccess);
        Assert.Equal(RoleNames.Master, result.Value.Name);
    }

    [Fact]
    public void Create_WhenNameMissing_ReturnsRequiredValueError()
    {
        var result = Role.Create(" ");

        Assert.True(result.IsFailure);
        Assert.Equal(GeneralErrorCodes.ValueIsRequired, result.Error.Code);
        Assert.Equal(nameof(Role.Name), result.Error.Target);
    }
}
