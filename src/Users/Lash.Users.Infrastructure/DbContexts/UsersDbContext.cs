using Lash.Users.Domain;
using Lash.Users.Infrastructure.Identity;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lash.Users.Infrastructure.DbContexts;

public sealed class UsersDbContext(DbContextOptions<UsersDbContext> options)
    : IdentityDbContext<IdentityUserEntity, IdentityRoleEntity, Guid>(options)
{
    public DbSet<IdentityPermission> Permissions => Set<IdentityPermission>();
    public DbSet<IdentityRolePermission> RolePermissions => Set<IdentityRolePermission>();
    public DbSet<RefreshSession> RefreshSessions => Set<RefreshSession>();
    public DbSet<IdentityEmailRequest> IdentityEmailRequests => Set<IdentityEmailRequest>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");
        builder.AddTransactionalOutboxEntities();

        builder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
    }
}
