using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application.Features.Commands.ResetPassword;

public sealed record ResetPasswordCommand(string Email, string Code, string Password, string ConfirmPassword) : ICommand;
