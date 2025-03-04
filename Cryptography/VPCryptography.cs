﻿using System.Security.Cryptography;
using System.Text;

namespace CorrelationIdUpdateAPI.Cryptography
{
    public class SymCryptography
    {
        private const string rijndael = "rijndael";
        private string mKey = string.Empty;
        private string mSalt = string.Empty;
        private ServiceProviderEnum mAlgorithm;
        private SymmetricAlgorithm mCryptoService;
        private void SetLegalIV()
        {
            switch (mAlgorithm)
            {
                case ServiceProviderEnum.Rijndael:
                    mCryptoService.IV = new byte[] {15, 111, 19, 46, 53, 194, 205, 249, 5, 70,
                156, 234, 168, 75, 115, 204};
                    break;
                default:
                    mCryptoService.IV = new byte[] { 15, 111, 19, 46, 53, 194, 205, 249 };
                    break;
            }
        }
        public enum ServiceProviderEnum
        {
            Rijndael,
            RC2,
            DES,
            TripleDES
        }
        public SymCryptography()
        {
            mCryptoService = new RijndaelManaged();
            mCryptoService.Mode = CipherMode.CBC;
            mAlgorithm = ServiceProviderEnum.Rijndael;
        }
        public SymCryptography(ServiceProviderEnum serviceProvider)
        {
            switch (serviceProvider)
            {
                case ServiceProviderEnum.Rijndael:
                    mCryptoService = new RijndaelManaged();
                    mAlgorithm = ServiceProviderEnum.Rijndael;
                    break;
                case ServiceProviderEnum.RC2:
                    mCryptoService = new RC2CryptoServiceProvider();
                    mAlgorithm = ServiceProviderEnum.RC2;
                    break;
                case ServiceProviderEnum.DES:
                    mCryptoService = new DESCryptoServiceProvider();
                    mAlgorithm = ServiceProviderEnum.DES;
                    break;
                case ServiceProviderEnum.TripleDES:
                    mCryptoService = new TripleDESCryptoServiceProvider();
                    mAlgorithm = ServiceProviderEnum.TripleDES;
                    break;
            }
            mCryptoService.Mode = CipherMode.CBC;
        }
        public SymCryptography(string serviceProviderName)
        {

            try
            {

                {
                    switch (serviceProviderName.ToLower())
                    {
                        case "rijndael":
                            serviceProviderName = "Rijndael";
                            mAlgorithm = ServiceProviderEnum.Rijndael;
                            break;
                        case "rc2":
                            serviceProviderName = "RC2";
                            mAlgorithm = ServiceProviderEnum.RC2;
                            break;
                        case "des":
                            serviceProviderName = "DES";
                            mAlgorithm = ServiceProviderEnum.DES;
                            break;
                        case "tripledes":
                            serviceProviderName = "TripleDES";
                            mAlgorithm = ServiceProviderEnum.TripleDES;
                            break;
                    }
                    mCryptoService = (SymmetricAlgorithm)CryptoConfig.CreateFromName(serviceProviderName);
                    mCryptoService.Mode = CipherMode.CBC;
                }
            }
            catch
            {

                {
                    throw;
                }
            }
        }
        public virtual byte[] GetLegalKey()
        {
            if (mCryptoService.LegalKeySizes.Length > 0)
            {

                int keySize = mKey.Length * 8;

                int minSize = mCryptoService.LegalKeySizes[0].MinSize;

                int maxSize = mCryptoService.LegalKeySizes[0].MaxSize;

                int skipSize = mCryptoService.LegalKeySizes[0].SkipSize;
                if (keySize > maxSize)
                {
                    mKey = mKey.Substring(0, maxSize / 8);
                }
                else
                {
                    if (keySize < maxSize)
                    {

                        int validSize = (keySize <= minSize) ? minSize : (keySize - keySize % skipSize) + skipSize;

                        if (keySize < validSize)
                        {
                            mKey = mKey.PadRight(validSize / 8, '*');
                        }
                    }
                }
            }

            PasswordDeriveBytes key = new PasswordDeriveBytes(mKey, ASCIIEncoding.UTF8.GetBytes(mSalt));
            return key.GetBytes(mKey.Length);
        }
        public virtual string Encrypt(string plainText)
        {

            byte[] plainByte = ASCIIEncoding.UTF8.GetBytes(plainText);

            byte[] keyByte = GetLegalKey();
            mCryptoService.Key = keyByte;
            SetLegalIV();

            ICryptoTransform cryptoTransform = mCryptoService.CreateEncryptor();

            MemoryStream ms = new MemoryStream();

            CryptoStream cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write);
            cs.Write(plainByte, 0, plainByte.Length);
            cs.FlushFinalBlock();

            byte[] cryptoByte = ms.ToArray();
            return Convert.ToBase64String(cryptoByte, 0, cryptoByte.GetLength(0));
        }
        public virtual string Decrypt(string cryptoText)
        {

            byte[] cryptoByte = Convert.FromBase64String(cryptoText);

            byte[] keyByte = GetLegalKey();
            mCryptoService.Key = keyByte;
            SetLegalIV();

            ICryptoTransform cryptoTransform = mCryptoService.CreateDecryptor();
            try
            {

                {

                    MemoryStream ms = new MemoryStream(cryptoByte, 0, cryptoByte.Length);

                    CryptoStream cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Read);

                    StreamReader sr = new StreamReader(cs);
                    return sr.ReadToEnd();
                }
            }
            catch
            {

                {
                    return null;
                }
            }
        }
        public string Key
        {
            get
            {
                return mKey;
            }
            set
            {
                mKey = value;
            }
        }
        public string Salt
        {
            get
            {
                return mSalt;
            }
            set
            {
                mSalt = value;
            }
        }
    }
    public class Hash
    {
        private string mSalt;
        private HashAlgorithm mCryptoService;
        public enum ServiceProviderEnum
        {
            SHA1,
            SHA256,
            SHA384,
            SHA512,
            MD5
        }
        public Hash()
        {
            mCryptoService = new SHA1Managed();
        }
        public Hash(ServiceProviderEnum serviceProvider)
        {
            switch (serviceProvider)
            {
                case ServiceProviderEnum.MD5:
                    mCryptoService = new MD5CryptoServiceProvider();
                    break;
                case ServiceProviderEnum.SHA1:
                    mCryptoService = new SHA1Managed();
                    break;
                case ServiceProviderEnum.SHA256:
                    mCryptoService = new SHA256Managed();
                    break;
                case ServiceProviderEnum.SHA384:
                    mCryptoService = new SHA384Managed();
                    break;
                case ServiceProviderEnum.SHA512:
                    mCryptoService = new SHA512Managed();
                    break;
            }
        }
        public Hash(string serviceProviderName)
        {
            try
            {

                {
                    mCryptoService = (HashAlgorithm)CryptoConfig.CreateFromName(serviceProviderName.ToUpper());
                }
            }
            catch
            {

                {
                    throw;
                }
            }
        }
        public virtual string Encrypt(string plainText)
        {

            byte[] cryptoByte = mCryptoService.ComputeHash(ASCIIEncoding.UTF8.GetBytes(plainText + mSalt));
            return Convert.ToBase64String(cryptoByte, 0, cryptoByte.Length);
        }
        public string Salt
        {
            get
            {
                return mSalt;
            }
            set
            {
                mSalt = value;
            }
        }
    }

}
