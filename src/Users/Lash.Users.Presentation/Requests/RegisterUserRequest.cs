using Lash.Users.Application.Features.Commands.RegisterClient;
using Lash.Users.Application.Features.Commands.RegisterMaster;

namespace Lash.Users.Presentation.Requests;

public sealed record RegisterUserRequest(
    string Email,
    string Password,
    string ConfirmPassword)
{
    public RegisterClientCommand ToRegisterClientCommand() =>
        new(Email, Password, ConfirmPassword);

    public RegisterMasterCommand ToRegisterMasterCommand() =>
        new(Email, Password, ConfirmPassword);
}
