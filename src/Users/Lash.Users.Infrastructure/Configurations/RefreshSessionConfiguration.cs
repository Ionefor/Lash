using Lash.Users.Domain;
using Lash.Users.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lash.Users.Infrastructure.Configurations;

public sealed class RefreshSessionConfiguration : IEntityTypeConfiguration<RefreshSession>
{
    public void Configure(EntityTypeBuilder<RefreshSession> builder)
    {
        builder.ToTable("refresh_sessions");

        builder.Property(session => session.TokenHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(session => session.AbsoluteExpiresAt)
            .IsRequired();

        builder.HasIndex(session => session.Jti)
            .IsUnique();

        builder.HasIndex(session => session.TokenHash)
            .IsUnique();

        builder.HasOne<IdentityUserEntity>()
            .WithMany()
            .HasForeignKey(session => session.UserId)
            .HasConstraintName("fk_refresh_sessions_users_user_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
