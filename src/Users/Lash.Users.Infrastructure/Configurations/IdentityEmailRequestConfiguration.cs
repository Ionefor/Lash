using Lash.Users.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lash.Users.Infrastructure.Configurations;

public sealed class IdentityEmailRequestConfiguration : IEntityTypeConfiguration<IdentityEmailRequest>
{
    public void Configure(EntityTypeBuilder<IdentityEmailRequest> builder)
    {
        builder.ToTable("identity_email_requests");
        builder.Property(request => request.EmailHash).HasMaxLength(64).IsRequired();
        builder.Property(request => request.Operation).HasMaxLength(32).IsRequired();
        builder.HasIndex(request => new { request.EmailHash, request.Operation, request.RequestedAt });
    }
}
