using System.Security.Cryptography;
using System.Text;

namespace Lash.Users.Infrastructure.Authorization;

internal static class RefreshTokenHasher
{
    public static string Hash(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexStringLower(bytes);
    }
}
