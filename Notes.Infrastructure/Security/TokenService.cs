using System.Security.Cryptography;
using System.Text;
using Notes.Application.Abstractions;

namespace Notes.Infrastructure.Security;

public sealed class TokenService : ITokenService
{
    private readonly string pepper;

    public TokenService(string pepper)
    {
        this.pepper = pepper ?? throw new ArgumentNullException(nameof(pepper));
    }

    public string CreateAccessToken(int userId)
    {
        // opaque token (no embedded claims)
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Base64UrlEncode(bytes);
    }

    public string HashToken(string token)
    {
        var data = Encoding.UTF8.GetBytes(token + pepper);
        var hash = SHA256.HashData(data);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
