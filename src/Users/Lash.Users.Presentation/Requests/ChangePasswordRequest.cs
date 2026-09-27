using Lash.Users.Application.Features.Commands.ChangePassword;

namespace Lash.Users.Presentation.Requests;

public sealed record ChangePasswordRequest(string CurrentPassword, string Password, string ConfirmPassword)
{
    public ChangePasswordCommand ToCommand(Guid userId) =>
        new(userId, CurrentPassword, Password, ConfirmPassword);
}
