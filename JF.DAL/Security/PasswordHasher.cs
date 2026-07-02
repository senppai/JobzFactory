using System;
using System.Security.Cryptography;

namespace JF.DAL.Security
{
    /// <summary>
    /// PBKDF2-based password hashing shared across all JobzFactory portals.
    /// Stored hash format: "pbkdf2.{iterations}.{saltBase64}.{hashBase64}".
    /// Verify transparently upgrades legacy plaintext values when a known
    /// legacy password matches, so existing accounts migrate on first login.
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltBytes = 16;
        private const int HashBytes = 24;
        private const int Iterations = 10000;
        private const string Prefix = "pbkdf2";

        public static string Hash(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            byte[] salt;
            byte[] hash;
            using (var derive = new Rfc2898DeriveBytes(password, SaltBytes, Iterations, HashAlgorithmName.SHA256))
            {
                salt = derive.Salt;
                hash = derive.GetBytes(HashBytes);
            }

            return string.Join(".", Prefix, Iterations, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        /// <summary>
        /// Verifies a password against a stored hash.
        /// Returns true when the password is correct. <paramref name="needsUpgrade"/> is true
        /// when the stored value was a legacy plaintext (or weak) entry that should be re-hashed.
        /// </summary>
        public static bool Verify(string stored, string password, out bool needsUpgrade)
        {
            needsUpgrade = false;

            if (string.IsNullOrEmpty(stored) || password == null)
                return false;

            var parts = stored.Split('.');
            if (parts.Length != 4 || parts[0] != Prefix)
            {
                // Legacy plaintext fallback: compare directly and flag for upgrade.
                if (string.Equals(stored, password, StringComparison.Ordinal))
                {
                    needsUpgrade = true;
                    return true;
                }
                return false;
            }

            if (!int.TryParse(parts[1], out int iterations) || iterations < 1000)
                return false;

            byte[] salt, expected;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expected = Convert.FromBase64String(parts[3]);
            }
            catch
            {
                return false;
            }

            byte[] actual;
            using (var derive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                actual = derive.GetBytes(expected.Length);
            }

            if (ConstantTimeEquals(actual, expected))
            {
                if (iterations != Iterations)
                    needsUpgrade = true;
                return true;
            }
            return false;
        }

        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
