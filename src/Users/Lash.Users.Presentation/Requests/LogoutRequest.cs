using Lash.Users.Application.Features.Commands.Logout;

namespace Lash.Users.Presentation.Requests;

public sealed record LogoutRequest(string RefreshToken)
{
    public LogoutCommand ToCommand() => new(RefreshToken);
}
