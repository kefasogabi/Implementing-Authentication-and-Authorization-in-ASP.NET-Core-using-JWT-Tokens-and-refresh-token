using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace JwtAuth.Helpers
{
    public class HashingHelper
    {
        public static string HashUsingPbkdf2(string password, string salt)
        {
            try
            {
                using var bytes = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 10000, HashAlgorithmName.SHA256);
                var derivedRandomKey = bytes.GetBytes(32);
                var hash = Convert.ToBase64String(derivedRandomKey);
                return hash;
            }
            catch (Exception ex)
            {
                string sMEssage = ex.Message;
                return string.Empty;
            }
        }
    }
}
