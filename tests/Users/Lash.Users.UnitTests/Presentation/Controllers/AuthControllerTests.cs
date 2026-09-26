using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Features.Commands.Login;
using Lash.Users.Application.Features.Commands.Logout;
using Lash.Users.Application.Features.Commands.Refresh;
using Lash.Users.Application.Models;
using Lash.Users.Presentation.Controllers;
using Lash.Users.Presentation.Requests;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebFlow.Abstractions.Interfaces;
using WebFlow.AspNetCore.Models;

namespace Lash.Users.UnitTests.Presentation.Controllers;

public sealed class AuthControllerTests
{
    [Fact]
    public async Task Login_WhenHandlerSucceeds_ReturnsOkEnvelopeWithTokens()
    {
        var tokens = new AuthTokens("access", "refresh");
        var handler = new Mock<ICommandHandler<LoginCommand, AuthTokens>>();
        handler.Setup(item => item.Handle(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<AuthTokens, ErrorList>(tokens));

        var result = await new AuthController().Login(new LoginRequest("user@example.com", "Password1!"), handler.Object, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(tokens, Assert.IsType<Envelope<AuthTokens>>(ok.Value).Data);
        handler.Verify(item => item.Handle(new LoginCommand("user@example.com", "Password1!"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_WhenHandlerFails_ReturnsErrorResponse()
    {
        var handler = FailedHandler<LoginCommand, AuthTokens>();

        var result = await new AuthController().Login(new LoginRequest("user@example.com", "Password1!"), handler.Object, CancellationToken.None);

        Assert.IsAssignableFrom<ObjectResult>(result);
    }

    [Fact]
    public async Task Refresh_WhenHandlerSucceeds_ReturnsOkEnvelopeWithTokens()
    {
        var tokens = new AuthTokens("access", "refresh");
        var handler = new Mock<ICommandHandler<RefreshTokensCommand, AuthTokens>>();
        handler.Setup(item => item.Handle(It.IsAny<RefreshTokensCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<AuthTokens, ErrorList>(tokens));

        var result = await new AuthController().Refresh(new RefreshTokensRequest("access", "refresh"), handler.Object, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(tokens, Assert.IsType<Envelope<AuthTokens>>(ok.Value).Data);
        handler.Verify(item => item.Handle(new RefreshTokensCommand("access", "refresh"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Refresh_WhenHandlerFails_ReturnsErrorResponse()
    {
        var handler = FailedHandler<RefreshTokensCommand, AuthTokens>();

        var result = await new AuthController().Refresh(new RefreshTokensRequest("access", "refresh"), handler.Object, CancellationToken.None);

        Assert.IsAssignableFrom<ObjectResult>(result);
    }

    [Fact]
    public async Task Logout_WhenHandlerSucceeds_PassesRefreshTokenToHandler()
    {
        var handler = SuccessfulHandler<LogoutCommand>();

        var result = await new AuthController().Logout(new LogoutRequest("refresh"), handler.Object, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        handler.Verify(item => item.Handle(new LogoutCommand("refresh"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Logout_WhenHandlerFails_ReturnsErrorResponse()
    {
        var handler = FailedUnitHandler<LogoutCommand>();

        var result = await new AuthController().Logout(new LogoutRequest("refresh"), handler.Object, CancellationToken.None);

        Assert.IsAssignableFrom<ObjectResult>(result);
    }

    private static Mock<ICommandHandler<TCommand>> SuccessfulHandler<TCommand>() where TCommand : ICommand
    {
        var handler = new Mock<ICommandHandler<TCommand>>();
        handler.Setup(item => item.Handle(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UnitResult.Success<ErrorList>());
        return handler;
    }

    private static Mock<ICommandHandler<TCommand, TResult>> FailedHandler<TCommand, TResult>() where TCommand : ICommand
    {
        var handler = new Mock<ICommandHandler<TCommand, TResult>>();
        handler.Setup(item => item.Handle(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<TResult, ErrorList>(new ErrorList([GeneralErrors.ValueIsInvalid("value")])));
        return handler;
    }

    private static Mock<ICommandHandler<TCommand>> FailedUnitHandler<TCommand>() where TCommand : ICommand
    {
        var handler = new Mock<ICommandHandler<TCommand>>();
        handler.Setup(item => item.Handle(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UnitResult.Failure(new ErrorList([GeneralErrors.ValueIsInvalid("value")])));
        return handler;
    }
}
