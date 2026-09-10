using Lash.Users.Application.Features.Commands.RegisterClient;
using Lash.Users.Application.Features.Commands.RegisterMaster;
using Lash.Users.Presentation.Requests;
using Microsoft.AspNetCore.Mvc;
using WebFlow.Abstractions.Interfaces;
using WebFlow.AspNetCore.Controllers;

namespace Lash.Users.Presentation.Controllers;

[ApiController]
[Route("registration")]
public sealed class RegistrationController : ApplicationController
{
    [HttpPost("client")]
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
