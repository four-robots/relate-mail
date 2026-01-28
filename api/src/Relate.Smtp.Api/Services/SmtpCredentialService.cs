using System.Security.Cryptography;

namespace Relate.Smtp.Api.Services;

public class SmtpCredentialService
{
    private const int ApiKeyBytes = 32;
    private const int BCryptWorkFactor = 11;

    public string GenerateApiKey()
    {
        var bytes = new byte[ApiKeyBytes];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCryptWorkFactor);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
