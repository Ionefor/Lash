namespace Lash.Users.Infrastructure.Identity;

public sealed class IdentityRolePermission
{
    private IdentityRolePermission()
    {
    }

    private IdentityRolePermission(Guid roleId, Guid permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }

    public Guid RoleId { get; private set; }
    public IdentityRoleEntity Role { get; private set; } = null!;
    public Guid PermissionId { get; private set; }
    public IdentityPermission Permission { get; private set; } = null!;

    public static IdentityRolePermission Create(Guid roleId, Guid permissionId) => new(roleId, permissionId);
}
