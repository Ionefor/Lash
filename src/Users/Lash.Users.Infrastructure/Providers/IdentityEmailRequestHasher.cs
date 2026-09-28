using System.Security.Cryptography;
using System.Text;

namespace Lash.Users.Infrastructure.Providers;

internal static class IdentityEmailRequestHasher
{
    public static string Hash(string email) => Convert.ToHexString(SHA256.HashData(
        Encoding.UTF8.GetBytes(email.Trim().ToUpperInvariant())));
}
