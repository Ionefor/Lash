using Lash.Users.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lash.Users.Infrastructure.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<IdentityPermission>
{
    public void Configure(EntityTypeBuilder<IdentityPermission> builder)
    {
        builder.ToTable("permissions");

        builder.Property(permission => permission.Code)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(permission => permission.Code)
            .IsUnique();
    }
}
