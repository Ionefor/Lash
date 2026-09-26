using Lash.Users.Application.Features.Commands.Refresh;

namespace Lash.Users.Presentation.Requests;

public sealed record RefreshTokensRequest(string AccessToken, string RefreshToken)
{
    public RefreshTokensCommand ToCommand() => new(AccessToken, RefreshToken);
}
