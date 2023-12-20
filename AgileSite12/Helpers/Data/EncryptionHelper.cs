using System;
using System.Security.Cryptography;
using System.Text;

using CMS.Base;

using SystemIO = System.IO;

namespace CMS.Helpers
{
    /// <summary>
    /// Provides validation RSA singed license key.
    /// </summary>
    public static class EncryptionHelper
    {
        #region "Constants"

        /// <summary>
        /// Prefix used in encrypt/decrypt methods to identify encrypted data.
        /// </summary>
        private const string ENCRYPT_PREFIX = "rijndael";


        /// <summary>
        /// Key used for data encryption in <see cref="EncryptData"/> and <see cref="DecryptData"/> methods.
        /// </summary>
        private const string KEY = "aW+gNdzvdt/zR1qzShQwHvHR77A8rTdzx1PfbHI/bTM=";


        /// <summary>
        /// Init vector used for data encryption in <see cref="EncryptData"/> and <see cref="DecryptData"/> methods.
        /// </summary>
        private const string IV = "KYYOEHKmq4gYfS2b2+lPQg==";

        #endregion


        #region "Variables"

        private static bool? mCheckVersionKey = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether Kentico version is displayed only if valid key is provided.
        /// </summary>
        private static bool CheckVersionKey
        {
            get
            {
                if (mCheckVersionKey == null)
                {
                    mCheckVersionKey = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSCheckVersionKey"], true);
                }
                return mCheckVersionKey.Value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Verifies RSA signed key which is used for getting Kentico version.
        /// </summary>
        /// <param name="inputKey">License key</param>
        public static bool VerifyVersionRSA(string inputKey)
        {
            // Version check is disabled
            if (!CheckVersionKey)
            {
                return true;
            }

            // If key is not entered
            if (string.IsNullOrEmpty(inputKey))
            {
                return false;
            }

            // Get plain text string
            string text = "domain:" + RequestContext.CurrentDomain;

            // Get key
            byte[] keyBytes = null;
            try
            {
                keyBytes = Convert.FromBase64String(inputKey);
            }
            catch
            {
                return false;
            }

            // Verify
            using (RSACryptoServiceProvider myRsaProvider = new RSACryptoServiceProvider())
            {
                myRsaProvider.FromXmlString(Encoding.ASCII.GetString(Convert.FromBase64String("PFJTQUtleVZhbHVlPjxNb2R1bHVzPjlQNWNmSTM1ZHJiZWVHcEUyRW44NEg1aFJLZDhBQlFPT1pOZ0dsUWhoK2V5dTU0dnorTldDc0cwd2FOaC9xei8wdnpvdktNWVcvWEszQ0xWS3FNekNFUkJFQlR6SzlVenV5Qk9BVm5yQ1dGM3ZaNkh3ZnR6WXZxdEVISjFGRDhidXA2RVhnVEFPTWNTSTBUaWo0NkJrOWU4cXBtaCttdWwraThaMHBQakxlU21pUXN5WUsxbWtqanNkTkNobUZ0dW1BVURwMEZ0WXd4QTU1L25mNkJjZFNOV1BPaHhubWVDOFZTTDRmME10WHQ0WTR0Y2hoTWFqVkhEZ0VXZjNFaWxzRHYrL1FwbFo0ZzBLOTliZ1AzZnQ5bUVGRkNLR0psNlhwOXRTVUc3cXpXUldsMnF2VHpNMVZjVEtSQk1oQmhqVUpQa2NjOXRBTnZkL3p4cDliallpUT09PC9Nb2R1bHVzPjxFeHBvbmVudD5BUUFCPC9FeHBvbmVudD48L1JTQUtleVZhbHVlPg==")));

                using (SHA1CryptoServiceProvider sha1CSP = new SHA1CryptoServiceProvider())
                {
                    return myRsaProvider.VerifyData(Encoding.ASCII.GetBytes(text), sha1CSP, keyBytes);
                }
            }
        }


        /// <summary>
        /// Encrypts data using symmetric cryptography. Data can by decrypted by calling <see cref="DecryptData"/> method. If null or empty string is on input, method returns it.
        /// The encrypted data is returned as a base 64 string.
        /// </summary>
        /// <param name="plainString">Data to encrypt.</param>
        [Obsolete("Use custom implementation instead.")]
        public static string EncryptData(string plainString)
        {
            if (string.IsNullOrEmpty(plainString))
            {
                return plainString;
            }

            // Prepare data
            byte[] toEncrypt = Encoding.UTF8.GetBytes(ENCRYPT_PREFIX + plainString);

            byte[] encryptedData = EncryptDataCore(toEncrypt, Convert.FromBase64String(KEY), Convert.FromBase64String(IV));

            // Return encrypted data
            return Convert.ToBase64String(encryptedData);
        }


        /// <summary>
        /// Decrypts data encrypted by <see cref="EncryptData"/> method.
        /// The input string is returned as is, if it is null or empty string or it was not encrypted using <see cref="EncryptData"/> method.
        /// </summary>
        /// <param name="encryptedString">Encrypted data as a base 64 string.</param>
        public static string DecryptData(string encryptedString)
        {
            if (string.IsNullOrEmpty(encryptedString))
            {
                return encryptedString;
            }

            try
            {
                byte[] encryptedData = Convert.FromBase64String(encryptedString);
                byte[] decryptedData = DecryptDataCore(encryptedData, Convert.FromBase64String(KEY), Convert.FromBase64String(IV));

                // Get decrypted data as a string
                string decryptedString = Encoding.UTF8.GetString(decryptedData);

                if (decryptedString.StartsWithCSafe(ENCRYPT_PREFIX))
                {
                    // Return decrypted data
                    return decryptedString.Substring(ENCRYPT_PREFIX.Length).TrimEnd('\0');
                }
                return encryptedString;
            }
            catch (Exception)
            {
                return encryptedString;
            }
        }


        /// <summary>
        /// Encrypts data using symmetric cryptography.
        /// </summary>
        private static byte[] EncryptDataCore(byte[] data, byte[] key, byte[] iv)
        {
            using (SystemIO.MemoryStream memory = new SystemIO.MemoryStream())
            {
                using (Rijndael provider = new RijndaelManaged())
                {
                    using (ICryptoTransform encryptor = provider.CreateEncryptor(key, iv))
                    {
                        using (CryptoStream crypto = new CryptoStream(memory, encryptor, CryptoStreamMode.Write))
                        {
                            crypto.Write(data, 0, data.Length);
                            crypto.FlushFinalBlock();

                            // Get encrypted data from memory stream
                            byte[] array = new byte[memory.Length];
                            memory.Seek(0, SystemIO.SeekOrigin.Begin);
                            memory.Read(array, 0, (int)memory.Length);

                            return array;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Decrypts data using symmetric cryptography.
        /// </summary>
        private static byte[] DecryptDataCore(byte[] data, byte[] key, byte[] iv)
        {
            using (SystemIO.MemoryStream memory = new SystemIO.MemoryStream())
            {
                using (Rijndael provider = new RijndaelManaged())
                {
                    using (ICryptoTransform decryptor = provider.CreateDecryptor(key, iv))
                    {
                        using (CryptoStream crypto = new CryptoStream(memory, decryptor, CryptoStreamMode.Write))
                        {
                            crypto.Write(data, 0, data.Length);
                            crypto.FlushFinalBlock();

                            // Get decrypted data from memory stream
                            byte[] array = new byte[memory.Length];
                            memory.Seek(0, SystemIO.SeekOrigin.Begin);
                            memory.Read(array, 0, (int)memory.Length);

                            return array;
                        }
                    }
                }
            }
        }

        #endregion
    }
}