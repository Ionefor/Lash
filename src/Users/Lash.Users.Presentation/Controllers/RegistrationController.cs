using Asp.Versioning;
using Lash.Users.Application.Features.Commands.RegisterClient;
using Lash.Users.Application.Features.Commands.RegisterMaster;
using Lash.Users.Presentation.RateLimiting;
using Lash.Users.Presentation.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Presentation.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/registration")]
public sealed class RegistrationController : UsersApplicationController
{
    [HttpPost("client")]
    [AllowAnonymous]
    [EnableRateLimiting(UsersRateLimitPolicies.Registration)]
    public async Task<IActionResult> RegisterClient(
        [FromBody] RegisterUserRequest request,
        [FromServices] ICommandHandler<RegisterClientCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request.ToRegisterClientCommand(), cancellationToken);

        if (result.IsFailure)
        {
            return Error(result.Error);
        }

        return CreatedEnvelope(result.Value);
    }

    [HttpPost("master")]
    [AllowAnonymous]
    [EnableRateLimiting(UsersRateLimitPolicies.Registration)]
    public async Task<IActionResult> RegisterMaster(
        [FromBody] RegisterUserRequest request,
        [FromServices] ICommandHandler<RegisterMasterCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request.ToRegisterMasterCommand(), cancellationToken);

        if (result.IsFailure)
        {
            return Error(result.Error);
        }

        return CreatedEnvelope(result.Value);
    }
}
