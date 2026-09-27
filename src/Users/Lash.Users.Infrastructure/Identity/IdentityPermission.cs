namespace Lash.Users.Infrastructure.Identity;

public sealed class IdentityPermission
{
    private readonly List<IdentityRolePermission> _rolePermissions = [];

    private IdentityPermission()
    {
    }

    private IdentityPermission(string code)
    {
        Id = Guid.NewGuid();
        Code = code;
    }

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public IReadOnlyCollection<IdentityRolePermission> RolePermissions => _rolePermissions;

    public static IdentityPermission Create(string code) => new(code);
}
