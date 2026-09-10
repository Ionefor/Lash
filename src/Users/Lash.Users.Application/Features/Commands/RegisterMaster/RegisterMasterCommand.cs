using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application.Features.Commands.RegisterMaster;

public sealed record RegisterMasterCommand(
    string Email,
    string Password,
    string ConfirmPassword) : ICommand;
