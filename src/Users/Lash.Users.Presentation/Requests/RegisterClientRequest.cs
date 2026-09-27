using Lash.Users.Application.Features.Commands.RegisterClient;

namespace Lash.Users.Presentation.Requests;

public sealed record RegisterClientRequest(
    string Email,
    string Password,
    string ConfirmPassword)
{
    public RegisterClientCommand ToCommand() =>
        new(Email, Password, ConfirmPassword);
}
