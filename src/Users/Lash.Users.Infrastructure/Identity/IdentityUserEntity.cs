using Microsoft.AspNetCore.Identity;

namespace Lash.Users.Infrastructure.Identity;

public sealed class IdentityUserEntity : IdentityUser<Guid>
{
    private readonly List<IdentityRoleEntity> _roles = [];

    private IdentityUserEntity()
    {
    }

    private IdentityUserEntity(string email)
    {
        Id = Guid.NewGuid();
        UserName = email;
        Email = email;
    }

    public IReadOnlyCollection<IdentityRoleEntity> Roles => _roles;

    public static IdentityUserEntity Create(string email) => new(email);
}
