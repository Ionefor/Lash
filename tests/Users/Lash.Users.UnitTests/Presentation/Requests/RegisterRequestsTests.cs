using Lash.Users.Presentation.Requests;

namespace Lash.Users.UnitTests.Presentation.Requests;

public sealed class RegisterRequestsTests
{
    [Fact]
    public void RegisterClientRequest_ToCommand_MapsAllRequestValues()
    {
        var request = new RegisterClientRequest(
            "client@example.com",
            "Password1!",
            "Password1!");

        var command = request.ToCommand();

        Assert.Equal("client@example.com", command.Email);
        Assert.Equal("Password1!", command.Password);
        Assert.Equal("Password1!", command.ConfirmPassword);
    }

    [Fact]
    public void RegisterMasterRequest_ToCommand_MapsAllRequestValues()
    {
        var request = new RegisterMasterRequest(
            "master@example.com",
            "Password1!",
            "Password1!");

        var command = request.ToCommand();

        Assert.Equal("master@example.com", command.Email);
        Assert.Equal("Password1!", command.Password);
        Assert.Equal("Password1!", command.ConfirmPassword);
    }
}
