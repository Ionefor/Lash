using Lash.Users.Application.Features.Commands.RegisterMaster;

namespace Lash.Users.Presentation.Requests;

public sealed record RegisterMasterRequest(
    string Email,
    string Password,
    string ConfirmPassword)
{
    public RegisterMasterCommand ToCommand() =>
        new(Email, Password, ConfirmPassword);
}
