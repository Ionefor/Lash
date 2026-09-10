using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application.Features.Commands.RegisterClient;

public sealed record RegisterClientCommand(
    string Email,
    string Password,
    string ConfirmPassword) : ICommand;
