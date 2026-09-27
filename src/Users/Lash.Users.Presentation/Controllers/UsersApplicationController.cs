using Lash.Users.Application.Constants;
using WebFlow.AspNetCore.Controllers;

namespace Lash.Users.Presentation.Controllers;

public abstract class UsersApplicationController : ApplicationController
{
    protected bool TryGetUserId(out Guid userId) => Guid.TryParse(
        User.FindFirst(AccessTokenClaimTypes.Sub)?.Value,
        out userId);
}
