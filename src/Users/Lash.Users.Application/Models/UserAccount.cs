namespace Lash.Users.Application.Models;

public sealed record UserAccount(Guid Id, string? Email, bool EmailConfirmed);
