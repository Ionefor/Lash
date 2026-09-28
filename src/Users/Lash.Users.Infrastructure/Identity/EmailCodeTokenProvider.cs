using Microsoft.AspNetCore.Identity;

namespace Lash.Users.Infrastructure.Identity;

public sealed class EmailCodeTokenProvider<TUser> : TotpSecurityStampBasedTokenProvider<TUser>
    where TUser : class
{
    public const string ProviderName = "EmailCode";

    public override async Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
    {
        var email = await manager.GetEmailAsync(user);
        return !string.IsNullOrWhiteSpace(email);
    }

    public override async Task<string> GetUserModifierAsync(string purpose, UserManager<TUser> manager, TUser user)
    {
        var email = await manager.GetEmailAsync(user);
        return $"EmailCode:{purpose}:{email}";
    }
}
