using Lash.Users.Domain;
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

        builder.HasIndex(session => session.Jti)
            .IsUnique();

        builder.HasIndex(session => session.TokenHash)
            .IsUnique();

        builder.HasOne(session => session.User)
            .WithMany()
            .HasForeignKey(session => session.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
