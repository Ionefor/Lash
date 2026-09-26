using Lash.Users.Application.Features.Commands.ResetPassword;

namespace Lash.Users.Presentation.Requests;

public sealed record ResetPasswordRequest(string Email, string Code, string Password, string ConfirmPassword)
{
    public ResetPasswordCommand ToCommand() => new(Email, Code, Password, ConfirmPassword);
}
