﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using CMS.Base;
using CMS.Modules;

namespace CMS.ModuleLicenses
{
    /// <summary>
    /// Helper class providing API for managing module licenses.
    /// </summary>
    public static class ModuleLicensesHelper
    {
        private static SafeDictionary<string, IEnumerable<string>> mLicensesDataCache;
        

        /// <summary>
        /// Cache for valid module licenses data.
        /// </summary>
        private static SafeDictionary<string, IEnumerable<string>> LicensesDataCache
        {
            get
            {
                return mLicensesDataCache ?? (mLicensesDataCache = new SafeDictionary<string, IEnumerable<string>>());
            }
        }


        /// <summary>
        /// Creates new private and public key pair for generating and validating module licenses. Created keys are encoded to Base64.
        /// </summary>
        /// <param name="privateKey">Created private key for generating module licenses</param>
        /// <param name="publicKey">Created public key for verifying module licenses</param>
        public static void GenerateKeyPair(out string privateKey, out string publicKey)
        {
            using (RSACryptoServiceProvider myRsaProvider = new RSACryptoServiceProvider())
            {
                privateKey = myRsaProvider.ToXmlString(true);
                publicKey = myRsaProvider.ToXmlString(false);
            }
            
            // Encode keys to Base64
            privateKey = Convert.ToBase64String(Encoding.ASCII.GetBytes(privateKey));
            publicKey = Convert.ToBase64String(Encoding.ASCII.GetBytes(publicKey));
        }


        /// <summary>
        /// Creates a module license containing given module license data and signature based on given private key.
        /// </summary>
        /// <param name="licenseData">Data to be stored in module license</param>
        /// <param name="privateKey">Private key, generated by <see cref="GenerateKeyPair"/> used for signing module license data</param>
        /// <returns>Module license containing given module license data and signature</returns>
        /// <exception cref="ArgumentNullException"><paramref name="licenseData"/> or <paramref name="privateKey"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="privateKey"/> has invalid format of private key encoded in Base64</exception>
        public static string CreateModuleLicense(string licenseData, string privateKey)
        {
            if (licenseData == null)
            {
                throw new ArgumentNullException("licenseData");
            }

            if (privateKey == null)
            {
                throw new ArgumentNullException("privateKey");
            }

            string license = "";

            using (RSACryptoServiceProvider myRsaProvider = new RSACryptoServiceProvider())
            {
                // If private key is not valid XML after converting from Base64, an exception is thrown
                try
                {
                    myRsaProvider.FromXmlString(Encoding.ASCII.GetString(Convert.FromBase64String(privateKey)));
                }
                catch (Exception e)
                {
                    throw new ArgumentException("Argument privateKey has incorrect format", "privateKey", e);
                }

                // Create license signature
                byte[] message = Encoding.ASCII.GetBytes(licenseData);
                var hash = myRsaProvider.SignData(message, new SHA512CryptoServiceProvider());

                // The delimiter between the license key data and signature is a newline character
                license = licenseData + Environment.NewLine + Convert.ToBase64String(hash);
            }

            return license;
        }


        /// <summary>
        /// Returns collection of valid module license data for given module.
        /// </summary>
        /// <param name="moduleName">Code name of module which licenses to get</param>
        /// <param name="publicKey">Public key, generated by <see cref="GenerateKeyPair"/> for validating licenses of given module</param>
        /// <returns>Collection of valid module license data for given module</returns>
        /// <exception cref="ArgumentNullException"><paramref name="moduleName"/> or <paramref name="publicKey"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="moduleName"/>is not valid name of existing module or <paramref name="publicKey"/> has invalid format of public key encoded in Base64</exception>
        public static IEnumerable<string> GetValidModuleLicenses(string moduleName, string publicKey)
        {
            if (moduleName == null)
            {
                throw new ArgumentNullException("moduleName");
            }

            if (publicKey == null)
            {
                throw new ArgumentNullException("publicKey");
            }

            // Try to load license data from cache
            string cacheKey = moduleName + "_" + publicKey;
            IEnumerable<string> licenses;
            if (LicensesDataCache.TryGetValue(cacheKey, out licenses))
            {
                return licenses;
            }

            // Get ID of given module
            var resource = ResourceInfoProvider.GetResourceInfo(moduleName);
            if (resource == null)
            {
                throw new ArgumentException("Argument moduleName does not contain name of existing module", "moduleName");
            }

            // Load module license keys from database
            var moduleLicenseKeys = ModuleLicenseKeyInfoProvider.GetResourceModuleLicenseKeyInfos(resource.ResourceID);

            List<string> validModuleLicenseData = new List<string>();
            foreach (var moduleLicenseKey in moduleLicenseKeys)
            {
                // Parse module licenses
                string licenseData = "";
                string signature = "";

                if (ParseModuleLicense(moduleLicenseKey.ModuleLicenseKeyLicense, out licenseData, out signature))
                {
                    // Get only valid license data
                    if (IsModuleLicenseValid(licenseData, signature, publicKey))
                    {
                        validModuleLicenseData.Add(licenseData);
                    }
                }
            }

            // Save valid license data to cache
            LicensesDataCache[cacheKey] = validModuleLicenseData;

            return validModuleLicenseData;
        }


        /// <summary>
        /// Validates module license data and it's signature with given public key.
        /// </summary>
        /// <param name="licenseData">License data to validate</param>
        /// <param name="signature">License signature to validate</param>
        /// <param name="publicKey">Public key, generated by <see cref="GenerateKeyPair"/> for validation</param>
        /// <returns>True whether license data and signature is valid, otherwise false</returns>
        /// <exception cref="ArgumentException"><paramref name="publicKey"/> has invalid format of public key encoded in Base64</exception>
        internal static bool IsModuleLicenseValid(string licenseData, string signature, string publicKey)
        {
            using (RSACryptoServiceProvider myRsaProvider = new RSACryptoServiceProvider())
            {
                try
                {
                    myRsaProvider.FromXmlString(Encoding.ASCII.GetString(Convert.FromBase64String(publicKey)));
                }
                catch (Exception e)
                {
                    throw new ArgumentException("Argument publicKey has invalid format of publicKey encoded in Base64", "publicKey", e);
                }

                try
                {
                    byte[] inputTextBytes = Encoding.ASCII.GetBytes(licenseData);
                    byte[] inputSignatureBytes = Convert.FromBase64String(signature);
                    return myRsaProvider.VerifyData(inputTextBytes, new SHA512CryptoServiceProvider(), inputSignatureBytes);
                }
                catch
                {
                    return false;
                }
            }
        }


        /// <summary>
        /// Parse given module license to license data and it's signature.
        /// </summary>
        /// <param name="license">Module license to parse</param>
        /// <param name="licenseData">Parsed data of module license</param>
        /// <param name="signature">Parsed signature of module license</param>
        /// <returns>True whether module license has been parsed successfully, otherwise false</returns>
        internal static bool ParseModuleLicense(string license, out string licenseData, out string signature)
        {
            // Find data and signature delimiter
            var delimiterPosition = license.LastIndexOf(Environment.NewLine, StringComparison.Ordinal);
            if (delimiterPosition < 0)
            {
                licenseData = "";
                signature = "";

                return false;
            }

            // Parse data and signature
            licenseData = license.Substring(0, delimiterPosition);
            signature = license.Substring(delimiterPosition + Environment.NewLine.Length);

            return true;
        }


        /// <summary>
        /// Clears cache for storing valid module license data.
        /// </summary>
        internal static void ClearCache()
        {
            LicensesDataCache.Clear();
        }
    }
}