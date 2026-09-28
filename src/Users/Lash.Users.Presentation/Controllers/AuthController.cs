using Asp.Versioning;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Features.Commands.Login;
using Lash.Users.Application.Features.Commands.Logout;
using Lash.Users.Application.Features.Commands.Refresh;
using Lash.Users.Application.Features.Commands.ChangePassword;
using Lash.Users.Application.Features.Commands.ConfirmEmail;
using Lash.Users.Application.Features.Commands.RequestPasswordReset;
using Lash.Users.Application.Features.Commands.ResendEmailConfirmation;
using Lash.Users.Application.Features.Commands.ResetPassword;
using Lash.Users.Application.Features.Queries.GetCurrentUser;
using Lash.Users.Application.Models;
using Lash.Users.Presentation.RateLimiting;
using Lash.Users.Presentation.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Presentation.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : UsersApplicationController
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(UsersRateLimitPolicies.Login)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] ICommandHandler<LoginCommand, AuthTokens> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request.ToCommand(), cancellationToken);

        if (result.IsFailure)
        {
            return Error(result.Error);
        }

        return OkEnvelope(result.Value);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(UsersRateLimitPolicies.Refresh)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokensRequest request,
        [FromServices] ICommandHandler<RefreshTokensCommand, AuthTokens> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request.ToCommand(), cancellationToken);

        if (result.IsFailure)
        {
            return Error(result.Error);
        }

        return OkEnvelope(result.Value);
    }

    [HttpPost("email/confirm")]
    [AllowAnonymous]
    [EnableRateLimiting(UsersRateLimitPolicies.IdentityPublic)]
    public async Task<IActionResult> ConfirmEmail(
        [FromBody] ConfirmEmailRequest request,
        [FromServices] ICommandHandler<ConfirmEmailCommand> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request.ToCommand(), cancellationToken);

        if (result.IsFailure)
        {
            return Error(result.Error);
        }

        return NoContent();
    }

    [HttpPost("email/confirmation/resend")]
    [AllowAnonymous]
    [EnableRateLimiting(UsersRateLimitPolicies.IdentityPublic)]
    public async Task<IActionResult> ResendEmailConfirmation(
        [FromBody] ResendEmailConfirmationRequest request,
        [FromServices] ICommandHandler<ResendEmailConfirmationCommand> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request.ToCommand(), cancellationToken);

        if (result.IsFailure)
        {
            return Error(result.Error);
        }

        return NoContent();
    }

    [HttpPost("password/reset/request")]
    [AllowAnonymous]
    [EnableRateLimiting(UsersRateLimitPolicies.IdentityPublic)]
    public async Task<IActionResult> RequestPasswordReset(
        [FromBody] RequestPasswordResetRequest request,
        [FromServices] ICommandHandler<RequestPasswordResetCommand> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request.ToCommand(), cancellationToken);

        if (result.IsFailure)
        {
            return Error(result.Error);
        }

        return NoContent();
    }

    [HttpPost("password/reset")]
    [AllowAnonymous]
    [EnableRateLimiting(UsersRateLimitPolicies.IdentityPublic)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        [FromServices] ICommandHandler<ResetPasswordCommand> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request.ToCommand(), cancellationToken);

        if (result.IsFailure)
        {
            return Error(result.Error);
        }

        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    [EnableRateLimiting(UsersRateLimitPolicies.Refresh)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        [FromServices] ICommandHandler<LogoutCommand> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request.ToCommand(), cancellationToken);

        if (result.IsFailure)
        {
            return Error(result.Error);
        }

        return NoContent();
    }

    [Authorize]
    [HttpPost("password/change")]
    [EnableRateLimiting(UsersRateLimitPolicies.ChangePassword)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        [FromServices] ICommandHandler<ChangePasswordCommand> handler,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Error(AuthErrors.Unauthorized().ToErrorList());
        }

        var result = await handler.Handle(request.ToCommand(userId), cancellationToken);

        if (result.IsFailure)
        {
            return Error(result.Error);
        }

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(
        [FromServices] IQueryHandler<GetCurrentUserQuery, UserProfile> handler,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Error(AuthErrors.Unauthorized().ToErrorList());
        }

        var result = await handler.Handle(new GetCurrentUserQuery(userId), cancellationToken);

        if (result.IsFailure)
        {
            return Error(result.Error);
        }

        return OkEnvelope(result.Value);
    }
}
