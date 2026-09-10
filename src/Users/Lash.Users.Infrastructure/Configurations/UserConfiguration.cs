using Lash.Users.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lash.Users.Infrastructure.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasMany(user => user.Roles)
            .WithMany(role => role.Users)
            .UsingEntity<IdentityUserRole<Guid>>();
    }
}
