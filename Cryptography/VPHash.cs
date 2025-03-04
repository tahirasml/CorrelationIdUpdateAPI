﻿using System.Security.Cryptography;

namespace CorrelationIdUpdateAPI.Cryptography
{
    public class VPHash
    {
        private VPHashType mbytHashType;
        private string mstrOriginalString;
        private string mstrHashString;
        private HashAlgorithm mhash;
        private bool mboolUseSalt;
        private string mstrSaltvalue = string.Empty;
        private short msrtSaltLength = 8;
        public enum VPHashType : byte
        {
            MD5,
            SHA1,
            SHA256,
            SHA384,
            SHA512
        }
        VPHashType HashType
        {
            get
            {
                return mbytHashType;
            }
            set
            {
                if (mbytHashType != value)
                {
                    mbytHashType = value;
                    mstrOriginalString = string.Empty;
                    mstrHashString = string.Empty;
                    this.SetEncryptor();
                }
            }
        }
        string Saltvalue
        {
            get
            {
                return mstrSaltvalue;
            }
            set
            {
                mstrSaltvalue = value;
            }
        }
        short SaltLength
        {
            get
            {
                return msrtSaltLength;
            }
            set
            {
                msrtSaltLength = value;
            }
        }
        bool UseSalt
        {
            get
            {
                return mboolUseSalt;
            }
            set
            {
                mboolUseSalt = value;
            }
        }
        HashAlgorithm HashObject
        {
            get
            {
                return mhash;
            }
            set
            {
                mhash = value;
            }
        }
        string OriginalString
        {
            get
            {
                return mstrOriginalString;
            }
            set
            {
                mstrOriginalString = value;
            }
        }
        string HashString
        {
            get
            {
                return mstrHashString;
            }
            set
            {
                mstrHashString = value;
            }
        }
        public VPHash()
        {
            mbytHashType = VPHashType.MD5;
        }
        public VPHash(VPHashType HashType)
        {
            mbytHashType = HashType;
        }
        public VPHash(VPHashType HashType, string OriginalString)
        {
            mbytHashType = HashType;
            mstrOriginalString = OriginalString;
        }
        public VPHash(VPHashType HashType, string OriginalString, bool UseSalt, string Saltvalue)
        {
            mbytHashType = HashType;
            mstrOriginalString = OriginalString;
            mboolUseSalt = UseSalt;
            mstrSaltvalue = Saltvalue;
        }
        private void SetEncryptor()
        {
            switch (mbytHashType)
            {
                case VPHashType.MD5:
                    mhash = new MD5CryptoServiceProvider();
                    break;
                case VPHashType.SHA1:
                    mhash = new SHA1CryptoServiceProvider();
                    break;
                case VPHashType.SHA256:
                    mhash = new SHA256Managed();
                    break;
                case VPHashType.SHA384:
                    mhash = new SHA384Managed();
                    break;
                case VPHashType.SHA512:
                    mhash = new SHA512Managed();
                    break;
            }
        }
        public string CreateHash()
        {

            byte[] bytvalue;

            byte[] bytHash;
            SetEncryptor();
            if (mboolUseSalt)
            {
                if (mstrSaltvalue.Length == 0)
                {
                    mstrSaltvalue = this.CreateSalt();
                }
            }
            bytvalue = System.Text.Encoding.UTF8.GetBytes(mstrSaltvalue + mstrOriginalString);
            bytHash = mhash.ComputeHash(bytvalue);
            return Convert.ToBase64String(bytHash);
        }
        public string CreateHash(string OriginalString)
        {
            mstrOriginalString = OriginalString;
            return this.CreateHash();
        }
        public string CreateHash(string OriginalString, VPHashType HashType)
        {
            mstrOriginalString = OriginalString;
            mbytHashType = HashType;
            return this.CreateHash();
        }
        public string CreateHash(string OriginalString, bool UseSalt)
        {
            mstrOriginalString = OriginalString;
            mboolUseSalt = UseSalt;
            return this.CreateHash();
        }
        public string CreateHash(string OriginalString, VPHashType HashType, bool UseSalt)
        {
            mstrOriginalString = OriginalString;
            mbytHashType = HashType;
            mboolUseSalt = UseSalt;
            return this.CreateHash();
        }
        public string CreateHash(string OriginalString, VPHashType HashType, string Saltvalue)
        {
            mstrOriginalString = OriginalString;
            mbytHashType = HashType;
            mstrSaltvalue = Saltvalue;
            return this.CreateHash();
        }
        public string CreateHash(string OriginalString, string Saltvalue)
        {
            mstrOriginalString = OriginalString;
            mstrSaltvalue = Saltvalue;
            return this.CreateHash();
        }
        public void Reset()
        {
            mstrSaltvalue = string.Empty;
            mstrOriginalString = string.Empty;
            mstrHashString = string.Empty;
            mboolUseSalt = false;
            mbytHashType = VPHashType.MD5;
            mhash = null;
        }
        public string CreateSalt()
        {

            byte[] bytSalt = new byte[msrtSaltLength + 1];

            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytSalt);
            return Convert.ToBase64String(bytSalt);
        }
    }

}
