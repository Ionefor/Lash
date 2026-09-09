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

    private User(string userName, string email, Role role)
    {
        Id = Guid.NewGuid();
        UserName = userName;
        Email = email;
        _roles.Add(role);
    }

    public IReadOnlyCollection<Role> Roles => _roles;

    public static Result<User, Error> RegisterClient(string userName, string email, Role role) =>
        Create(userName, email, role, RoleNames.Client);

    public static Result<User, Error> RegisterMaster(string userName, string email, Role role) =>
        Create(userName, email, role, RoleNames.Master);

    public static Result<User, Error> CreateAdmin(string userName, string email, Role role) =>
        Create(userName, email, role, RoleNames.Admin);

    private static Result<User, Error> Create(string userName, string email, Role? role, string requiredRoleName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return Result.Failure<User, Error>(GeneralErrors.ValueIsRequired(nameof(UserName)));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<User, Error>(GeneralErrors.ValueIsRequired(nameof(Email)));
        }

        if (role is null || !string.Equals(role.Name, requiredRoleName, StringComparison.Ordinal))
        {
            return Result.Failure<User, Error>(AuthErrors.RoleIsInvalid(requiredRoleName));
        }

        return Result.Success<User, Error>(new User(userName, email, role));
    }
}
