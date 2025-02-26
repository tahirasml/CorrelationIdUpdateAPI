using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CorrelationIdUpdateAPI.Cryptography
{
    public class ConnectionStringSecurity
    {
        private static byte[] kKey = Encoding.ASCII.GetBytes("4ksBNfdLJM0Nf43uP1ViLH3D"); // 24-byte key
        private static byte[] kIV = Encoding.ASCII.GetBytes("init vec"); // 8-byte IV

        /// <summary>
        /// Encrypts a plain text string using TripleDES.
        /// </summary>
        public static string Encrypt(string plainText)
        {
            byte[] encryptedBytes = Encrypt(Encoding.UTF8.GetBytes(plainText), kKey);
            return Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        /// Encrypts a byte array using TripleDES.
        /// </summary>
        private static byte[] Encrypt(byte[] bytesData, byte[] bytesKey)
        {
            MemoryStream memoryStream = new MemoryStream();

            ICryptoTransform cryptoServiceProvider = GetCryptoServiceProvider(bytesKey, true); // true for encryption
            CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoServiceProvider, CryptoStreamMode.Write);
            try
            {
                cryptoStream.Write(bytesData, 0, bytesData.Length);
                cryptoStream.FlushFinalBlock();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while encrypting data: {ex.Message}");
            }
            finally
            {
                cryptoStream.Close();
            }

            return memoryStream.ToArray();
        }

        /// <summary>
        /// Decrypts an encrypted string using TripleDES.
        /// </summary>
        public static string Decrypt(string cipherText)
        {
            string decryptedText = Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(cipherText), kKey));
            return decryptedText;
        }

        /// <summary>
        /// Decrypts a byte array using TripleDES.
        /// </summary>
        private static byte[] Decrypt(byte[] bytesData, byte[] bytesKey)
        {
            MemoryStream memoryStream = new MemoryStream();

            ICryptoTransform cryptoServiceProvider = GetCryptoServiceProvider(bytesKey, false); // false for decryption
            CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoServiceProvider, CryptoStreamMode.Write);
            try
            {
                cryptoStream.Write(bytesData, 0, bytesData.Length);
                cryptoStream.FlushFinalBlock();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while decrypting data: {ex.Message}");
            }
            finally
            {
                cryptoStream.Close();
            }

            return memoryStream.ToArray();
        }

        /// <summary>
        /// Creates a TripleDES CryptoServiceProvider.
        /// </summary>
        private static ICryptoTransform GetCryptoServiceProvider(byte[] bytesKey, bool isEncrypt)
        {
            TripleDES tripleDES = new TripleDESCryptoServiceProvider()
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };

            return isEncrypt ? tripleDES.CreateEncryptor(bytesKey, kIV) : tripleDES.CreateDecryptor(bytesKey, kIV);
        }
    }
}