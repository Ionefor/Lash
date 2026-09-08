using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;

namespace Lash.Users.Domain;

public sealed class Permission
{
    private readonly List<RolePermission> _rolePermissions = [];

    private Permission()
    {
    }

    private Permission(string code)
    {
        Id = Guid.NewGuid();
        Code = code;
    }

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions;

    public static Result<Permission, Error> Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<Permission, Error>(GeneralErrors.ValueIsRequired(nameof(Code)));
        }

        return Result.Success<Permission, Error>(new Permission(code));
    }
}
