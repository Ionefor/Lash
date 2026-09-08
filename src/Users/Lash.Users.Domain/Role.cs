using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Microsoft.AspNetCore.Identity;

namespace Lash.Users.Domain;

public sealed class Role : IdentityRole<Guid>
{
    private readonly List<User> _users = [];
    private readonly List<RolePermission> _rolePermissions = [];

    private Role()
    {
    }

    private Role(string name)
        : base(name)
    {
    }

    public IReadOnlyCollection<User> Users => _users;
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions;

    public static Result<Role, Error> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Role, Error>(GeneralErrors.ValueIsRequired(nameof(Name)));
        }

        return Result.Success<Role, Error>(new Role(name));
    }
}
