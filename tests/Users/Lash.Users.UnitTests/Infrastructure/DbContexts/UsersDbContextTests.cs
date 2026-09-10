using Lash.Users.Domain;
using Lash.Users.Infrastructure.DbContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lash.Users.UnitTests.Infrastructure.DbContexts;

public sealed class UsersDbContextTests
{
    [Fact]
    public void Model_MapsUsersRolesAndIdentityJoinTable()
    {
        using var context = CreateContext();

        var user = context.Model.FindEntityType(typeof(User))!;
        var role = context.Model.FindEntityType(typeof(Role))!;
        var userRole = context.Model.FindEntityType(typeof(IdentityUserRole<Guid>))!;

        Assert.NotNull(user);
        Assert.NotNull(role);
        Assert.NotNull(userRole);
        Assert.Equal("users", user!.GetTableName());
        Assert.Equal("roles", role!.GetTableName());
        Assert.Equal("user_roles", userRole!.GetTableName());
        Assert.NotNull(user.FindSkipNavigation(nameof(User.Roles)));
        Assert.NotNull(role.FindSkipNavigation(nameof(Role.Users)));
    }

    [Fact]
    public void Model_MapsModuleEntitiesAndUniqueSecurityIndexes()
    {
        using var context = CreateContext();

        var permission = context.Model.FindEntityType(typeof(Permission))!;
        var refreshSession = context.Model.FindEntityType(typeof(RefreshSession))!;
        var rolePermission = context.Model.FindEntityType(typeof(RolePermission))!;

        Assert.NotNull(permission);
        Assert.NotNull(refreshSession);
        Assert.NotNull(rolePermission);
        Assert.Equal("permissions", permission!.GetTableName());
        Assert.Contains(permission.GetIndexes(), index =>
            index.IsUnique && index.Properties.Single().Name == nameof(Permission.Code));
        Assert.Equal("refresh_sessions", refreshSession!.GetTableName());
        Assert.Contains(refreshSession.GetIndexes(), index =>
            index.IsUnique && index.Properties.Single().Name == nameof(RefreshSession.Jti));
        Assert.Equal("role_permissions", rolePermission!.GetTableName());
        Assert.Equal(
            [nameof(RolePermission.RoleId), nameof(RolePermission.PermissionId)],
            rolePermission.FindPrimaryKey()!.Properties.Select(property => property.Name));
    }

    private static UsersDbContext CreateContext() => new(
        new DbContextOptionsBuilder<UsersDbContext>()
            .UseNpgsql("Host=localhost;Database=lash_users;Username=test;Password=test")
            .Options);
}
