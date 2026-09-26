using Lash.Users.Application.Features.Commands.Login;

namespace Lash.Users.Presentation.Requests;

public sealed record LoginRequest(string Email, string Password)
{
    public LoginCommand ToCommand() => new(Email, Password);
}
