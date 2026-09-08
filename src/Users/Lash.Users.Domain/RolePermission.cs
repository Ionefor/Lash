using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;

namespace Lash.Users.Domain;

public sealed class RolePermission
{
    private RolePermission()
    {
    }

    private RolePermission(Guid roleId, Guid permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }

    public Guid RoleId { get; private set; }
    public Role Role { get; private set; } = null!;
    public Guid PermissionId { get; private set; }
    public Permission Permission { get; private set; } = null!;

    public static Result<RolePermission, Error> Create(Guid roleId, Guid permissionId)
    {
        if (roleId == Guid.Empty)
        {
            return Result.Failure<RolePermission, Error>(GeneralErrors.ValueIsInvalid(nameof(RoleId)));
        }

        if (permissionId == Guid.Empty)
        {
            return Result.Failure<RolePermission, Error>(GeneralErrors.ValueIsInvalid(nameof(PermissionId)));
        }

        return Result.Success<RolePermission, Error>(new RolePermission(roleId, permissionId));
    }
}
