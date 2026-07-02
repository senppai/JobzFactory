using System;
using System.Security.Cryptography;
using System.Text;

namespace JF.DAL.Security
{
    /// <summary>
    /// HMAC-SHA256 signed, time-limited tokens used to authorize cross-site CV
    /// downloads. The Recruteur portal mints a token for an application id; the
    /// public JobzFactory portal validates it before serving the file, so CVs
    /// are not retrievable by guessing sequential ids.
    /// Token format: "{id}.{expiryEpochSeconds}.{signatureBase64Url}".
    /// </summary>
    public static class CvToken
    {
        private static readonly Encoding Utf8 = new UTF8Encoding(false);

        public static string Generate(int applicationId, string secret, int ttlMinutes = 60)
        {
            if (string.IsNullOrEmpty(secret)) throw new ArgumentException("secret is required", nameof(secret));
            long expiry = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (ttlMinutes * 60L);
            string payload = applicationId + "." + expiry;
            string sig = Base64Url(Sign(payload, secret));
            return applicationId + "." + expiry + "." + sig;
        }

        public static bool Validate(string token, string secret, int applicationId)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(secret))
                return false;

            var parts = token.Split('.');
            if (parts.Length != 3) return false;
            if (!int.TryParse(parts[0], out int id) || id != applicationId) return false;
            if (!long.TryParse(parts[1], out long expiry)) return false;

            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiry)
                return false;

            string payload = id + "." + expiry;
            byte[] expected;
            try { expected = Base64UrlDecode(parts[2]); }
            catch { return false; }

            byte[] actual = Sign(payload, secret);
            return ConstantTimeEquals(actual, expected);
        }

        private static byte[] Sign(string payload, string secret)
        {
            using (var hmac = new HMACSHA256(Utf8.GetBytes(secret)))
            {
                return hmac.ComputeHash(Utf8.GetBytes(payload));
            }
        }

        private static string Base64Url(byte[] data)
        {
            return Convert.ToBase64String(data)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static byte[] Base64UrlDecode(string s)
        {
            string padded = s.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }
            return Convert.FromBase64String(padded);
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
