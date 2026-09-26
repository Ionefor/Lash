using System.Security.Claims;
using Lash.Users.Application.Models;
using Lash.Users.Presentation.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lash.Users.UnitTests.Presentation.Controllers;

public sealed class UsersApplicationControllerTests
{
    [Fact]
    public void TryGetUserId_WhenSubjectClaimIsGuid_ReturnsUserId()
    {
        var userId = Guid.NewGuid();
        var controller = CreateController(userId.ToString());

        var result = controller.GetUserId(out var actualUserId);

        Assert.True(result);
        Assert.Equal(userId, actualUserId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-guid")]
    public void TryGetUserId_WhenSubjectClaimIsMissingOrInvalid_ReturnsFalse(string? subject)
    {
        var controller = CreateController(subject);

        var result = controller.GetUserId(out _);

        Assert.False(result);
    }

    private static TestUsersApplicationController CreateController(string? subject)
    {
        var claims = subject is null ? [] : new[] { new Claim(AccessTokenClaimTypes.Subject, subject) };
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) };
        return new TestUsersApplicationController { ControllerContext = new ControllerContext { HttpContext = context } };
    }

    private sealed class TestUsersApplicationController : UsersApplicationController
    {
        public bool GetUserId(out Guid userId) => TryGetUserId(out userId);
    }
}
