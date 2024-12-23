﻿using System.Security.Cryptography;
using System.Text;

namespace CorrelationIdUpdateAPI.Cryptography
{
    public class ConnectionStringSecurity
    {
            private static byte[] kKey = Encoding.ASCII.GetBytes("4ksBNfdLJM0Nf43uP1ViLH3D");
            private static byte[] kIV = Encoding.ASCII.GetBytes("init vec");

            public static string Decrypt(string cipherText)
            {
                string str = Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(cipherText), kKey));
                return str;
            }

            public static byte[] Decrypt(byte[] bytesData, byte[] bytesKey)
            {
                MemoryStream memoryStream = new MemoryStream();

                ICryptoTransform cryptoServiceProvider = GetCryptoServiceProvider(bytesKey);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoServiceProvider, CryptoStreamMode.Write);
                try
                {
                    cryptoStream.Write(bytesData, 0, (int)bytesData.Length);
                }
                catch (Exception exception)
                {
                    throw new Exception(string.Concat("Error while writing encrypted data to the stream: \n", exception.Message));
                }
                cryptoStream.FlushFinalBlock();
                cryptoStream.Close();
                return memoryStream.ToArray();
            }

            public static ICryptoTransform GetCryptoServiceProvider(byte[] bytesKey)
            {
                ICryptoTransform cryptoTransform;
                TripleDES tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider()
                {
                    Mode = CipherMode.CBC
                };
                cryptoTransform = tripleDESCryptoServiceProvider.CreateDecryptor(bytesKey, kIV);

                return cryptoTransform;
            }
        }
    }
