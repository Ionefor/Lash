using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application.Features.Commands.ConfirmEmail;

public sealed record ConfirmEmailCommand(string Email, string Code) : ICommand;
