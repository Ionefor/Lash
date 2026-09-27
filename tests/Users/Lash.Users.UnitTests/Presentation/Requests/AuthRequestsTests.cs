using Lash.Users.Presentation.Requests;

namespace Lash.Users.UnitTests.Presentation.Requests;

public sealed class AuthRequestsTests
{
    [Fact]
    public void LogoutRequest_ToCommand_MapsRefreshToken()
    {
        var request = new LogoutRequest("refresh-token");

        var command = request.ToCommand();

        Assert.Equal("refresh-token", command.RefreshToken);
    }

    [Fact]
    public void ChangePasswordRequest_ToCommand_MapsUserIdAndPasswords()
    {
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest("Current1!", "Password1!", "Password1!");

        var command = request.ToCommand(userId);

        Assert.Equal(userId, command.UserId);
        Assert.Equal("Current1!", command.CurrentPassword);
        Assert.Equal("Password1!", command.Password);
        Assert.Equal("Password1!", command.ConfirmPassword);
    }
}
