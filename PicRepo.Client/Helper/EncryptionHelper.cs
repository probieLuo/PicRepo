using System;
using System.Security.Cryptography;
using System.Text;

namespace PicRepo.Client.Helper
{
    public static class EncryptionHelper
    {
        public static string EncryptString(string plain)
        {
            if (string.IsNullOrEmpty(plain)) return string.Empty;
            try
            {
                var bytes = Encoding.UTF8.GetBytes(plain);
                var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encrypted);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string DecryptString(string cipherBase64)
        {
            if (string.IsNullOrEmpty(cipherBase64)) return string.Empty;
            try
            {
                var encrypted = Convert.FromBase64String(cipherBase64);
                var bytes = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                // If not valid base64 or not protected with DPAPI, assume it's plain text and return as-is
                return cipherBase64;
            }
        }
    }
}
