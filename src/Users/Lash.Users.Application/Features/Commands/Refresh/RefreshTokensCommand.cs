using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application.Features.Commands.Refresh;

public sealed record RefreshTokensCommand(string AccessToken, string RefreshToken) : ICommand;
