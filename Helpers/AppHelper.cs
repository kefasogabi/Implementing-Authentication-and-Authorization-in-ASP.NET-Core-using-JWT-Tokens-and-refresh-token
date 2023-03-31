using System.Security.Cryptography;
using System.Text;

namespace JwtAuth.Helpers
{
    public class AppHelper
    {

        public static string GetNewGuid()
        {
            Guid _Guid = Guid.NewGuid();
            return _Guid.ToString();
        }

        public static string GetUnique8ByteKey()
        {
            try
            {
                int maxSize = 10;
                char[] chars = new char[62];
                string a;
                a = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
                chars = a.ToCharArray();
                int size = maxSize;
                byte[] data = new byte[1];
                RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
                crypto.GetNonZeroBytes(data);
                size = maxSize;
                data = new byte[size];
                crypto.GetNonZeroBytes(data);
                StringBuilder result = new StringBuilder(size);
                foreach (byte b in data)
                { result.Append(chars[b % (chars.Length - 1)]); }
                return result.ToString();
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}
