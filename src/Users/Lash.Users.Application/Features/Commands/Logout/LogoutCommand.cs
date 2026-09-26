using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application.Features.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken) : ICommand;
