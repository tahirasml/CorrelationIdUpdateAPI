﻿using System.Security.Cryptography;
using System.Text;

namespace CorrelationIdUpdateAPI.Cryptography
{
    public class ServiceCryptography
    {
        static private char[] alphabet =
           "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/="
               .ToCharArray();
        static private byte[] codes = new byte[256];
        public ServiceCryptography()
        {
        }

        /// <summary>
        /// This is method take three parameters 1- the string to be decrypted
        /// boolean variable if decryption request want hashing or not finally hash key used in hashing
        /// The algorithm used inside decryption is triple DES
        /// </summary>
        /// <param name="cipherString"></param>
        /// <param name="useHashing"></param>
        /// <param name="hashKey"></param>
        /// <returns></returns>
        public static string Decrypt(string cipherString, bool useHashing, string hashKey)
        {
            try
            {
                byte[] keyArray;
                byte[] toEncryptArray = Convert.FromBase64String(cipherString);


                string key = hashKey;//"Someth1ngSpeci@l";

                if (useHashing)
                {
                    MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                    keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    hashmd5.Clear();
                }
                else
                    keyArray = UTF8Encoding.UTF8.GetBytes(key);

                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();

                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = tdes.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                tdes.Clear();
                return UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception ex)
            {
                return "Decrypt Error" + ex.ToString();

            }
        }

        /// <summary>
        /// This is method take three parameters 1- the string to be encrypted
        /// boolean variable if encryption request want hashing or not finally hash key used in hashing
        /// The algorithm used inside encryption is triple DES
        /// </summary>
        /// <param name="toEncrypt"></param>
        /// <param name="useHashing"></param>
        /// <param name="hashKey"></param>
        /// <returns></returns>
        public static string Encrypt(string toEncrypt, bool useHashing, string hashKey)
        {
            try
            {
                byte[] keyArray;
                byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

                string key = hashKey;//"Someth1ngSpeci@l";

                if (useHashing)
                {
                    MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                    keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    hashmd5.Clear();
                }
                else
                    keyArray = UTF8Encoding.UTF8.GetBytes(key);

                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();

                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = tdes.CreateEncryptor();

                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                tdes.Clear();
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch (Exception)
            {
                return null;
            }
        }
        static public char[] encode(string strData)
        {
            byte[] data = GetBytes(strData);
            char[] output = new char[((data.Length + 2) / 3) * 4];

            //
            // 3 bytes encode to 4 chars.  Output is always an even
            // multiple of 4 characters.
            //
            for (int i = 0, index = 0; i < data.Length; i += 3, index += 4)
            {
                bool quad = false;
                bool trip = false;

                int val = (0xFF & (int)data[i]);
                val <<= 8;
                if ((i + 1) < data.Length)
                {
                    val |= (0xFF & (int)data[i + 1]);
                    trip = true;
                }
                val <<= 8;
                if ((i + 2) < data.Length)
                {
                    val |= (0xFF & (int)data[i + 2]);
                    quad = true;
                }
                output[index + 3] = alphabet[(quad ? (val & 0x3F) : 64)];
                val >>= 6;
                output[index + 2] = alphabet[(trip ? (val & 0x3F) : 64)];
                val >>= 6;
                output[index + 1] = alphabet[val & 0x3F];
                val >>= 6;
                output[index + 0] = alphabet[val & 0x3F];
            }

            StringBuilder outString = new StringBuilder(new string(output));
            outString.Insert(0, "%%");
            outString.Append("%%");
            String finalout = outString.ToString().Replace('=', '-');
            return finalout.ToCharArray();
        }

        /*
         decode string to bytes 
         */

        public static byte[] GetBytes(string text)
        {
            return ASCIIEncoding.UTF8.GetBytes(text);
        }


        static public byte[] decode(String re)
        {

            re = re.Replace("%", "");
            re.Replace('-', '=');
            char[] data = re.ToCharArray();
            // as our input could contain non-encode data (newlines,
            // whitespace of any sort, whatever) we must first adjust
            // our count of USABLE data so that...
            // (a) we don't misallocate the output array, and
            // (b) think that we miscalculated our data length
            //     just because of extraneous throw-away junk

            int tempLen = data.Length;
            for (int ix = 0; ix < data.Length; ix++)
            {
                if ((data[ix] > 255) || codes[data[ix]] < 0)
                    --tempLen;  // ignore non-valid chars and padding
            }
            // calculate required length:
            //  -- 3 bytes for every 4 valid encode chars
            //  -- plus 2 bytes if there are 3 extra encode chars,
            //     or plus 1 byte if there are 2 extra.

            int len = (tempLen / 4) * 3;
            if ((tempLen % 4) == 3) len += 2;
            if ((tempLen % 4) == 2) len += 1;

            byte[] output = new byte[len];



            int shift = 0;   // # of excess bits stored in accum
            int accum = 0;   // excess bits
            int index = 0;

            // we now go through the entire array (NOT using the 'tempLen' value)
            for (int ix = 0; ix < data.Length; ix++)
            {
                int value = (data[ix] > 255) ? -1 : codes[data[ix]];

                if (value >= 0)           // skip over non-code
                {
                    accum <<= 6;            // bits shift up by 6 each time thru
                    shift += 6;             // loop, with new bits being put in
                    accum |= value;         // at the bottom.
                    if (shift >= 8)       // whenever there are 8 or more shifted in,
                    {
                        shift -= 8;         // write them out (from the top, leaving any
                        output[index++] =      // excess at the bottom for next iteration.
                            (byte)((accum >> shift) & 0xff);
                    }
                }
                // we will also have skipped processing a padding null byte ('=') here;
                // these are used ONLY for padding to an even length and do not legally
                // occur as encoded data. for this reason we can ignore the fact that
                // no index++ operation occurs in that special case: the out[] array is
                // initialized to all-zero bytes to start with and that works to our
                // advantage in this combination.
            }

            // if there is STILL something wrong we just have to throw up now!
            if (index != output.Length)
            {
                throw new Exception("Miscalculated data length (wrote " + index + " instead of " + output.Length + ")");
            }

            return output;
        }

    }
}
