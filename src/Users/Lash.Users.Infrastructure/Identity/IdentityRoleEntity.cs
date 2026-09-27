using Microsoft.AspNetCore.Identity;

namespace Lash.Users.Infrastructure.Identity;

public sealed class IdentityRoleEntity : IdentityRole<Guid>
{
    private readonly List<IdentityUserEntity> _users = [];
    private readonly List<IdentityRolePermission> _rolePermissions = [];

    private IdentityRoleEntity()
    {
    }

    private IdentityRoleEntity(string name) : base(name)
    {
    }

    public IReadOnlyCollection<IdentityUserEntity> Users => _users;
    public IReadOnlyCollection<IdentityRolePermission> RolePermissions => _rolePermissions;

    public static IdentityRoleEntity Create(string name) => new(name);
}
