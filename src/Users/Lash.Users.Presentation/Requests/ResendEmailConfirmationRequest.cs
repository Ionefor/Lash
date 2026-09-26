using Lash.Users.Application.Features.Commands.ResendEmailConfirmation;

namespace Lash.Users.Presentation.Requests;

public sealed record ResendEmailConfirmationRequest(string Email)
{
    public ResendEmailConfirmationCommand ToCommand() => new(Email);
}
