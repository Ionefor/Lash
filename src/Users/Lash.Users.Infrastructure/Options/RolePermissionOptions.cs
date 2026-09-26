namespace Lash.Users.Infrastructure.Options;

public sealed class RolePermissionOptions
{
    public const string SectionName = "RolePermissions";

    public Dictionary<string, string[]> Permissions { get; init; } = [];
    public Dictionary<string, string[]> Roles { get; init; } = [];
}
