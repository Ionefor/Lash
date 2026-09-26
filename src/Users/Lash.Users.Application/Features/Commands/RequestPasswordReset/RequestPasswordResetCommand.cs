using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application.Features.Commands.RequestPasswordReset;

public sealed record RequestPasswordResetCommand(string Email) : ICommand;
