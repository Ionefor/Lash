namespace Lash.Users.Infrastructure.Seeding;

public sealed class UsersSeederRegistry(
    RolesSeeder rolesSeeder,
    PermissionsSeeder permissionsSeeder,
    AdminSeeder adminSeeder) : ISeederRegistry
{
    public IReadOnlyCollection<ISeeder> All { get; } =
    [
        rolesSeeder,
        permissionsSeeder,
        adminSeeder
    ];
}
