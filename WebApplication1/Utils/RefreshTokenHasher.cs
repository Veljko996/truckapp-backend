using System.Security.Cryptography;
using System.Text;

namespace WebApplication1.Utils;

/// <summary>
/// Hešuje refresh tokene pre čuvanja u bazi. Refresh token je 256-bitna
/// kriptografski slučajna vrednost (visoka entropija), pa je običan SHA-256
/// bez soli dovoljan i deterministički (potreban za lookup po hešu).
/// Sirovi token se nikad ne čuva u bazi — samo njegov heš.
/// </summary>
public static class RefreshTokenHasher
{
    public static string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
