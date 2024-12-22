using System.Security.Cryptography;
using System.Text;

namespace CorrelationIdUpdateAPI.Utilities
{
    public static class ConnectionStringEncryptor
    {
        private static readonly string EncryptionKey = "4ksBNfdLJM0Nf43uP1ViLH3D"; // Use a strong key or fetch from a secure source like environment variables

        public static string Encrypt(string plainText)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32).Substring(0, 32)); // 32-byte key for AES-256
            aes.IV = new byte[16]; // Default IV of zeros
            aes.Padding = PaddingMode.PKCS7; // Ensure padding is consistent

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Combine IV + encryptedBytes for storage
            byte[] combinedBytes = new byte[aes.IV.Length + encryptedBytes.Length];
            Array.Copy(aes.IV, 0, combinedBytes, 0, aes.IV.Length);
            Array.Copy(encryptedBytes, 0, combinedBytes, aes.IV.Length, encryptedBytes.Length);

            return Convert.ToBase64String(combinedBytes); // Encode as Base64 for storage
        }

        public static string Decrypt(string encryptedText)
        {
            byte[] combinedBytes = Convert.FromBase64String(encryptedText);

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32).Substring(0, 32)); // 32-byte key for AES-256
            aes.IV = new byte[16]; // Default IV of zeros
            aes.Padding = PaddingMode.PKCS7; // Ensure padding is consistent

            // Extract IV and encrypted data
            byte[] iv = new byte[16];
            byte[] encryptedBytes = new byte[combinedBytes.Length - iv.Length];
            Array.Copy(combinedBytes, 0, iv, 0, iv.Length);
            Array.Copy(combinedBytes, iv.Length, encryptedBytes, 0, encryptedBytes.Length);

            using var decryptor = aes.CreateDecryptor(aes.Key, iv);
            byte[] plainBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
