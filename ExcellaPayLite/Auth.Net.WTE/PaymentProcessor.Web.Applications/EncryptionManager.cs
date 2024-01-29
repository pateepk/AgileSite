namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Win32;
    using System.Security.Cryptography;

    public static class EncryptionManager
    {
        // for Production, if Connection String is encrypted, they private key is in Registry 
        private const string registryPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\PAYMENTPROCESSOR";
        private const string registryWebKey = "WebKey";
        private const string resgitryUserKey = "UserKey";
        private const string resgitryResetKey = "ResetKey";


        //private static string pwConnectionString = getWebKey();
        //private static string pwExternalPassword = getUserKey();
        //private static string pwResetPassword = getResetKey();

        private static string pwConnectionString = "WTEPaymentPr0c3ss0r111";
        private static string pwExternalPassword = "WTEPaymentPr0c3ss0r222";
        private static string pwResetPassword = "WTEPaymentPr0c3ss0r333";

        private static string getWebKey()
        {
            object obj = Registry.GetValue(registryPath, registryWebKey, null);
            string key = string.Empty;
            if (obj != null)
            {
                key = obj.ToString();
            }
            return key;
        }

        private static string getUserKey()
        {
            object obj = Registry.GetValue(registryPath, resgitryUserKey, null);
            string key = string.Empty;
            if (obj != null)
            {
                key = obj.ToString();
            }
            return key;
        }

        private static string getResetKey()
        {
            object obj = Registry.GetValue(registryPath, resgitryResetKey, null);
            string key = string.Empty;
            if (obj != null)
            {
                key = obj.ToString();
            }
            return key;
        }


        public static string EncryptString(string key, string stringToEncrypt)
        {
            return EncryptConnectionString(key, stringToEncrypt);
        }

        public static string DecryptString(string key, string stringToDecrypt)
        {
            return DecryptConnectionString(key, stringToDecrypt);
        }

        public static string EncryptConnectionString(string stringToEncrypt)
        {
            return EncryptConnectionString("", stringToEncrypt);
        }

        public static string EncryptExternalPassword(string externalPassword)
        {
            return EncryptConnectionString(pwExternalPassword, externalPassword);
        }

        public static string DecryptExternalPassword(string externalPassword)
        {
            return DecryptConnectionString(pwExternalPassword, externalPassword);
        }

        public static string EncryptResetPassword(string key)
        {
            return EncryptConnectionString(pwResetPassword, key);
        }

        public static string DecryptResetPassword(string key)
        {
            return DecryptConnectionString(pwResetPassword, key);
        }

        public static string EncryptConnectionString(string key, string stringToEncrypt)
        {
            if (String.IsNullOrEmpty(key))
            {
                key = pwConnectionString;
            }
            TripleDESCryptoServiceProvider encryptor;
            MD5CryptoServiceProvider hashProvider;
            byte[] hashPassword;
            byte[] byteArray;
            string encryptedString;

            if (stringToEncrypt == null || stringToEncrypt == string.Empty)
            { encryptedString = null; }
            else
            {
                hashProvider = new MD5CryptoServiceProvider();
                hashPassword = hashProvider.ComputeHash(ASCIIEncoding.ASCII.GetBytes(key));
                hashProvider = null;

                encryptor = new TripleDESCryptoServiceProvider();
                encryptor.Key = hashPassword;
                encryptor.Mode = CipherMode.ECB;

                byteArray = ASCIIEncoding.ASCII.GetBytes(stringToEncrypt);

                encryptedString = Convert.ToBase64String(encryptor.CreateEncryptor().TransformFinalBlock(byteArray, 0, byteArray.Length));
                encryptedString = encryptedString.Replace("=", "~");

                encryptor = null;
            }

            return encryptedString;
        }

        public static string DecryptString(string stringToDecrypt)
        {
            return DecryptConnectionString("", stringToDecrypt);
        }

        public static string DecryptConnectionString(string stringToDecrypt)
        {
            return DecryptConnectionString("", stringToDecrypt);
        }

        public static string DecryptConnectionString(string key, string stringToDecrypt)
        {
            if (String.IsNullOrEmpty(key))
            {
                key = pwConnectionString;
            }
            TripleDESCryptoServiceProvider encryptor;
            MD5CryptoServiceProvider hashProvider;
            byte[] hashPassword;
            byte[] buff;
            string decryptedString;

            if (stringToDecrypt == null || stringToDecrypt == string.Empty)
            { decryptedString = null; }
            else
            {
                stringToDecrypt = stringToDecrypt.Replace(" ", "+").Replace("~", "=");

                hashProvider = new MD5CryptoServiceProvider();
                hashPassword = hashProvider.ComputeHash(ASCIIEncoding.ASCII.GetBytes(key));
                hashProvider = null;

                encryptor = new TripleDESCryptoServiceProvider();
                encryptor.Key = hashPassword;
                encryptor.Mode = CipherMode.ECB;

                try
                {
                    buff = Convert.FromBase64String(stringToDecrypt);
                    decryptedString = ASCIIEncoding.ASCII.GetString(encryptor.CreateDecryptor().TransformFinalBlock(buff, 0, buff.Length));
                }
                catch (Exception e)
                {
                    ErrorManager.logError(String.Format(SV.ErrorMessages.EncryptionError, stringToDecrypt), e);
                    decryptedString = string.Empty;
                }
                encryptor = null;
            }
            return decryptedString;
        }
    }

}
