using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application.Features.Commands.ResendEmailConfirmation;

public sealed record ResendEmailConfirmationCommand(string Email) : ICommand;
