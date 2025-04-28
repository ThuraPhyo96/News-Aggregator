using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace NewsAggregator.Infrastructure.Helpers
{
    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            // Generate random 16 bytes salt
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            // Generate 32 bytes hash
            byte[] hash = KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                10000,
                32);

            // Combine salt + hash
            byte[] hashBytes = new byte[16 + 32];
            Buffer.BlockCopy(salt, 0, hashBytes, 0, 16);  // salt first
            Buffer.BlockCopy(hash, 0, hashBytes, 16, 32); // then hash

            return Convert.ToBase64String(hashBytes);
        }

        public static bool Verify(string password, string storedHashBase64)
        {
            byte[] hashBytes = Convert.FromBase64String(storedHashBase64);

            if (hashBytes.Length != 48) // 16 + 32
                throw new ArgumentException("Invalid hash length");

            // Extract salt and hash
            byte[] salt = new byte[16];
            byte[] storedHash = new byte[32];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, 16);
            Buffer.BlockCopy(hashBytes, 16, storedHash, 0, 32);

            // Hash the incoming password with the extracted salt
            byte[] newHash = KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                10000,
                32);

            // Compare securely
            return CryptographicOperations.FixedTimeEquals(newHash, storedHash);
        }
    }
}
