using Lash.Users.Application.Features.Commands.RequestPasswordReset;

namespace Lash.Users.Presentation.Requests;

public sealed record RequestPasswordResetRequest(string Email)
{
    public RequestPasswordResetCommand ToCommand() => new(Email);
}
