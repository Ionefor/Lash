using Lash.Users.Application.Features.Commands.ConfirmEmail;

namespace Lash.Users.Presentation.Requests;

public sealed record ConfirmEmailRequest(string Email, string Code)
{
    public ConfirmEmailCommand ToCommand() => new(Email, Code);
}
