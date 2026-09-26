namespace Lash.Users.Application.Models;

public sealed record UserProfile(Guid Id, string Email, IReadOnlyList<string> Roles);
