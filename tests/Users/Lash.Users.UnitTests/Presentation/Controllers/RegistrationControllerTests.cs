using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Application.Features.Commands.RegisterClient;
using Lash.Users.Application.Features.Commands.RegisterMaster;
using Lash.Users.Presentation.Controllers;
using Lash.Users.Presentation.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebFlow.Abstractions.Interfaces;
using WebFlow.AspNetCore.Models;

namespace Lash.Users.UnitTests.Presentation.Controllers;

public sealed class RegistrationControllerTests
{
    [Fact]
    public async Task RegisterClient_WhenHandlerSucceeds_ReturnsCreatedResponse()
    {
        var userId = Guid.NewGuid();
        var handler = new RegisterClientHandlerStub(userId);
        var controller = new RegistrationController();
        var request = new RegisterUserRequest(
            "client@example.com",
            "Password1!",
            "Password1!");

        var result = await controller.RegisterClient(request, handler, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Null(createdResult.Location);
        var envelope = Assert.IsType<Envelope<Guid>>(createdResult.Value);
        Assert.Equal(userId, envelope.Data);
        Assert.Equal("client@example.com", handler.ReceivedCommand!.Email);
    }

    [Fact]
    public async Task RegisterMaster_WhenHandlerSucceeds_ReturnsCreatedResponse()
    {
        var userId = Guid.NewGuid();
        var handler = new RegisterMasterHandlerStub(userId);
        var controller = new RegistrationController();
        var request = new RegisterUserRequest(
            "master@example.com",
            "Password1!",
            "Password1!");

        var result = await controller.RegisterMaster(request, handler, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Null(createdResult.Location);
        var envelope = Assert.IsType<Envelope<Guid>>(createdResult.Value);
        Assert.Equal(userId, envelope.Data);
        Assert.Equal("master@example.com", handler.ReceivedCommand!.Email);
    }

    private sealed class RegisterClientHandlerStub(Guid userId) : ICommandHandler<RegisterClientCommand, Guid>
    {
        public RegisterClientCommand? ReceivedCommand { get; private set; }

        public Task<Result<Guid, ErrorList>> Handle(
            RegisterClientCommand command,
            CancellationToken cancellationToken = default)
        {
            ReceivedCommand = command;
            return Task.FromResult(Result.Success<Guid, ErrorList>(userId));
        }
    }

    private sealed class RegisterMasterHandlerStub(Guid userId) : ICommandHandler<RegisterMasterCommand, Guid>
    {
        public RegisterMasterCommand? ReceivedCommand { get; private set; }

        public Task<Result<Guid, ErrorList>> Handle(
            RegisterMasterCommand command,
            CancellationToken cancellationToken = default)
        {
            ReceivedCommand = command;
            return Task.FromResult(Result.Success<Guid, ErrorList>(userId));
        }
    }
}
