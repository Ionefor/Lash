using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application.Features.Commands.ChangePassword;

public sealed record ChangePasswordCommand(Guid UserId, string CurrentPassword, string Password, string ConfirmPassword) : ICommand;
