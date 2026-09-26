using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application.Features.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand;
