using System.Security.Claims;
using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Features.Commands.ChangePassword;
using Lash.Users.Application.Features.Commands.ConfirmEmail;
using Lash.Users.Application.Features.Commands.RequestPasswordReset;
using Lash.Users.Application.Features.Commands.ResendEmailConfirmation;
using Lash.Users.Application.Features.Commands.ResetPassword;
using Lash.Users.Application.Features.Queries.GetCurrentUser;
using Lash.Users.Application.Models;
using Lash.Users.Presentation.Controllers;
using Lash.Users.Presentation.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebFlow.Abstractions.Interfaces;
using WebFlow.AspNetCore.Models;

namespace Lash.Users.UnitTests.Presentation.Controllers;

public sealed class AuthIdentityEndpointsTests
{
    [Fact]
    public async Task ConfirmEmail_WhenHandlerSucceeds_ReturnsNoContentAndMapsRequest()
    {
        var handler = SuccessfulHandler<ConfirmEmailCommand>();

        var result = await new AuthController().ConfirmEmail(new ConfirmEmailRequest("user@example.com", "code"), handler.Object, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        handler.Verify(item => item.Handle(new ConfirmEmailCommand("user@example.com", "code"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResendEmailConfirmation_WhenHandlerSucceeds_ReturnsNoContentAndMapsRequest()
    {
        var handler = SuccessfulHandler<ResendEmailConfirmationCommand>();

        var result = await new AuthController().ResendEmailConfirmation(new ResendEmailConfirmationRequest("user@example.com"), handler.Object, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        handler.Verify(item => item.Handle(new ResendEmailConfirmationCommand("user@example.com"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestPasswordReset_WhenHandlerSucceeds_ReturnsNoContentAndMapsRequest()
    {
        var handler = SuccessfulHandler<RequestPasswordResetCommand>();

        var result = await new AuthController().RequestPasswordReset(new RequestPasswordResetRequest("user@example.com"), handler.Object, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        handler.Verify(item => item.Handle(new RequestPasswordResetCommand("user@example.com"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_WhenHandlerSucceeds_ReturnsNoContentAndMapsRequest()
    {
        var handler = SuccessfulHandler<ResetPasswordCommand>();
        var request = new ResetPasswordRequest("user@example.com", "code", "Password1!", "Password1!");

        var result = await new AuthController().ResetPassword(request, handler.Object, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        handler.Verify(item => item.Handle(new ResetPasswordCommand("user@example.com", "code", "Password1!", "Password1!"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmEmail_WhenHandlerFails_ReturnsErrorResponse()
    {
        var handler = FailedHandler<ConfirmEmailCommand>();

        var result = await new AuthController().ConfirmEmail(new ConfirmEmailRequest("user@example.com", "code"), handler.Object, CancellationToken.None);

        Assert.IsAssignableFrom<ObjectResult>(result);
    }

    [Fact]
    public async Task ResendEmailConfirmation_WhenHandlerFails_ReturnsErrorResponse()
    {
        var handler = FailedHandler<ResendEmailConfirmationCommand>();

        var result = await new AuthController().ResendEmailConfirmation(new ResendEmailConfirmationRequest("user@example.com"), handler.Object, CancellationToken.None);

        Assert.IsAssignableFrom<ObjectResult>(result);
    }

    [Fact]
    public async Task RequestPasswordReset_WhenHandlerFails_ReturnsErrorResponse()
    {
        var handler = FailedHandler<RequestPasswordResetCommand>();

        var result = await new AuthController().RequestPasswordReset(new RequestPasswordResetRequest("user@example.com"), handler.Object, CancellationToken.None);

        Assert.IsAssignableFrom<ObjectResult>(result);
    }

    [Fact]
    public async Task ResetPassword_WhenHandlerFails_ReturnsErrorResponse()
    {
        var handler = FailedHandler<ResetPasswordCommand>();

        var result = await new AuthController().ResetPassword(new ResetPasswordRequest("user@example.com", "code", "Password1!", "Password1!"), handler.Object, CancellationToken.None);

        Assert.IsAssignableFrom<ObjectResult>(result);
    }

    [Fact]
    public async Task ChangePassword_WhenSubjectClaimIsValid_PassesUserIdAndPasswordsToHandler()
    {
        var userId = Guid.NewGuid();
        var handler = SuccessfulHandler<ChangePasswordCommand>();
        var request = new ChangePasswordRequest("Current1!", "Password1!", "Password1!");

        var result = await CreateController(userId).ChangePassword(request, handler.Object, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        handler.Verify(item => item.Handle(new ChangePasswordCommand(userId, "Current1!", "Password1!", "Password1!"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangePassword_WhenSubjectClaimIsMissing_ReturnsUnauthorizedWithoutCallingHandler()
    {
        var handler = SuccessfulHandler<ChangePasswordCommand>();

        var result = await CreateController(null).ChangePassword(new ChangePasswordRequest("Current1!", "Password1!", "Password1!"), handler.Object, CancellationToken.None);

        Assert.Equal(StatusCodes.Status401Unauthorized, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        handler.Verify(item => item.Handle(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetCurrentUser_WhenSubjectClaimIsValid_ReturnsOkEnvelopeAndPassesUserId()
    {
        var userId = Guid.NewGuid();
        var profile = new UserProfile(userId, "user@example.com", ["client"]);
        var handler = new Mock<IQueryHandler<GetCurrentUserQuery, UserProfile>>();
        handler.Setup(item => item.Handle(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<UserProfile, ErrorList>(profile));

        var result = await CreateController(userId).GetCurrentUser(handler.Object, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(profile, Assert.IsType<Envelope<UserProfile>>(ok.Value).Data);
        handler.Verify(item => item.Handle(new GetCurrentUserQuery(userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrentUser_WhenSubjectClaimIsInvalid_ReturnsUnauthorizedWithoutCallingHandler()
    {
        var handler = new Mock<IQueryHandler<GetCurrentUserQuery, UserProfile>>();
        var controller = CreateController(null);
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(AccessTokenClaimTypes.Subject, "invalid")]));

        var result = await controller.GetCurrentUser(handler.Object, CancellationToken.None);

        Assert.Equal(StatusCodes.Status401Unauthorized, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        handler.Verify(item => item.Handle(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static AuthController CreateController(Guid? userId)
    {
        var claims = userId is null ? [] : new[] { new Claim(AccessTokenClaimTypes.Subject, userId.Value.ToString()) };
        return new AuthController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
            }
        };
    }

    private static Mock<ICommandHandler<TCommand>> SuccessfulHandler<TCommand>() where TCommand : ICommand
    {
        var handler = new Mock<ICommandHandler<TCommand>>();
        handler.Setup(item => item.Handle(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UnitResult.Success<ErrorList>());
        return handler;
    }

    private static Mock<ICommandHandler<TCommand>> FailedHandler<TCommand>() where TCommand : ICommand
    {
        var handler = new Mock<ICommandHandler<TCommand>>();
        handler.Setup(item => item.Handle(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UnitResult.Failure(new ErrorList([GeneralErrors.ValueIsInvalid("value")])));
        return handler;
    }
}
