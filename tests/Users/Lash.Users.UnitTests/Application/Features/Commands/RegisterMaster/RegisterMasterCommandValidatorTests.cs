using Lash.Users.Application.Features.Commands.RegisterMaster;

namespace Lash.Users.UnitTests.Application.Features.Commands.RegisterMaster;

public sealed class RegisterMasterCommandValidatorTests
{
    [Fact]
    public void Validate_WhenCommandIsValid_ReturnsSuccess()
    {
        var validator = new RegisterMasterCommandValidator();
        var command = new RegisterMasterCommand(
            "master@example.com",
            "Password1!",
            "Password1!");

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
