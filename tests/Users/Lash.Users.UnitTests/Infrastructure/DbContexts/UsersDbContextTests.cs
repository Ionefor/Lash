using Lash.Users.Infrastructure.DbContexts;
using Lash.Users.Infrastructure.Identity;
using Lash.Users.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lash.Users.UnitTests.Infrastructure.DbContexts;

public sealed class UsersDbContextTests
{
    [Fact]
    public void Model_MapsUsersRolesAndIdentityJoinTable()
    {
        using var context = CreateContext();

        var user = context.Model.FindEntityType(typeof(IdentityUserEntity))!;
        var role = context.Model.FindEntityType(typeof(IdentityRoleEntity))!;
        var userRole = context.Model.FindEntityType(typeof(IdentityUserRole<Guid>))!;

        Assert.NotNull(user);
        Assert.NotNull(role);
        Assert.NotNull(userRole);
        Assert.Equal("users", user!.GetTableName());
        Assert.Equal("roles", role!.GetTableName());
        Assert.Equal("user_roles", userRole!.GetTableName());
        Assert.NotNull(user.FindSkipNavigation(nameof(IdentityUserEntity.Roles)));
        Assert.NotNull(role.FindSkipNavigation(nameof(IdentityRoleEntity.Users)));
    }

    [Fact]
    public void Model_MapsModuleEntitiesAndUniqueSecurityIndexes()
    {
        using var context = CreateContext();

        var permission = context.Model.FindEntityType(typeof(IdentityPermission))!;
        var refreshSession = context.Model.FindEntityType(typeof(RefreshSession))!;
        var identityEmailRequest = context.Model.FindEntityType(typeof(IdentityEmailRequest))!;
        var rolePermission = context.Model.FindEntityType(typeof(IdentityRolePermission))!;

        Assert.NotNull(permission);
        Assert.NotNull(refreshSession);
        Assert.NotNull(rolePermission);
        Assert.Equal("permissions", permission!.GetTableName());
        Assert.Contains(permission.GetIndexes(), index =>
            index.IsUnique && index.Properties.Single().Name == nameof(IdentityPermission.Code));
        Assert.Equal("refresh_sessions", refreshSession!.GetTableName());
        Assert.Contains(refreshSession.GetIndexes(), index =>
            index.IsUnique && index.Properties.Single().Name == nameof(RefreshSession.Jti));
        Assert.Equal("identity_email_requests", identityEmailRequest!.GetTableName());
        Assert.Contains(identityEmailRequest.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual(
                [nameof(IdentityEmailRequest.EmailHash), nameof(IdentityEmailRequest.Operation), nameof(IdentityEmailRequest.RequestedAt)]));
        Assert.Equal("role_permissions", rolePermission!.GetTableName());
        Assert.Equal(
            [nameof(IdentityRolePermission.RoleId), nameof(IdentityRolePermission.PermissionId)],
            rolePermission.FindPrimaryKey()!.Properties.Select(property => property.Name));
    }

    private static UsersDbContext CreateContext() => new(
        new DbContextOptionsBuilder<UsersDbContext>()
            .UseNpgsql("Host=localhost;Database=lash_users;Username=test;Password=test")
            .Options);
}
