using System.Security.Cryptography;

namespace Rbac.Utils;

public class PasswordHasher
{
    private const int SaltSize = 32;       // 256-bit
    private const int HashSize = 32;       // 256-bit
    private const int Iterations = 100_000;

    public (byte[] Hash, byte[] Salt) HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256
        );

        var hash = pbkdf2.GetBytes(HashSize);

        return (hash, salt);
    }

    public bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            storedSalt,
            Iterations,
            HashAlgorithmName.SHA256
        );

        var computedHash = pbkdf2.GetBytes(HashSize);

        return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
    }
}
