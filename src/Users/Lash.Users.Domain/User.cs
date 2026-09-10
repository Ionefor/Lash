using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Microsoft.AspNetCore.Identity;

namespace Lash.Users.Domain;

public sealed class User : IdentityUser<Guid>
{
    private readonly List<Role> _roles = [];

    private User()
    {
    }

    private User(string email, Role role)
    {
        Id = Guid.NewGuid();
        UserName = email;
        Email = email;
        _roles.Add(role);
    }

    public IReadOnlyCollection<Role> Roles => _roles;

    public static Result<User, Error> RegisterClient(string email, Role role) =>
        Create(email, role, RoleNames.Client);

    public static Result<User, Error> RegisterMaster(string email, Role role) =>
        Create(email, role, RoleNames.Master);

    public static Result<User, Error> CreateAdmin(string email, Role role) =>
        Create(email, role, RoleNames.Admin);

    private static Result<User, Error> Create(string email, Role? role, string requiredRoleName)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<User, Error>(GeneralErrors.ValueIsRequired(nameof(Email)));
        }

        if (role is null || !string.Equals(role.Name, requiredRoleName, StringComparison.Ordinal))
        {
            return Result.Failure<User, Error>(AuthErrors.RoleIsInvalid(requiredRoleName));
        }

        return Result.Success<User, Error>(new User(email, role));
    }
}
