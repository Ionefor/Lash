using Lash.Users.Presentation.Requests;

namespace Lash.Users.UnitTests.Presentation.Requests;

public sealed class RegisterUserRequestTests
{
    [Fact]
    public void ToRegisterClientCommand_WhenCalled_MapsAllRequestValues()
    {
        var request = new RegisterUserRequest(
            "client@example.com",
            "Password1!",
            "Password1!");

        var command = request.ToRegisterClientCommand();

        Assert.Equal("client@example.com", command.Email);
        Assert.Equal("Password1!", command.Password);
        Assert.Equal("Password1!", command.ConfirmPassword);
    }

    [Fact]
    public void ToRegisterMasterCommand_WhenCalled_MapsAllRequestValues()
    {
        var request = new RegisterUserRequest(
            "master@example.com",
            "Password1!",
            "Password1!");

        var command = request.ToRegisterMasterCommand();

        Assert.Equal("master@example.com", command.Email);
        Assert.Equal("Password1!", command.Password);
        Assert.Equal("Password1!", command.ConfirmPassword);
    }
}
