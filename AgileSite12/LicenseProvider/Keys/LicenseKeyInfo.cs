using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

[assembly: RegisterObjectType(typeof(LicenseKeyInfo), LicenseKeyInfo.OBJECT_TYPE)]

namespace CMS.LicenseProvider
{
    /// <summary>
    /// LicenseKeyInfo data container class.
    /// </summary>
    public class LicenseKeyInfo : AbstractInfo<LicenseKeyInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.licensekey";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(LicenseKeyInfoProvider), OBJECT_TYPE, "CMS.LicenseKey", "LicenseKeyID", null, null, "LicenseDomain", ObjectTypeInfo.COLUMN_NAME_UNKNOWN, null, null, null, null)
        {
            SupportsCloning = false,
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.None,
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                },
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                    {
                        new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                    }
            },

            LogEvents = false,
            SupportsVersioning = false,
            TouchCacheDependencies = true,
            ContainsMacros = false,
        };

        #endregion

        
        #region "Constants"

        /// <summary>
        /// Default timestamp format for license expiration date.
        /// </summary>
        public const string LICENSE_EXPIRATION_DATE_FORMAT = @"M\/d\/yyyy";

        #endregion


        #region "Variables"

        /// <summary>
        /// Unlimited license.
        /// </summary>
        public static DateTime TIME_UNLIMITED_LICENSE = DateTime.MinValue;

        /// <summary>
        /// Unlimited webfarm servers constant.
        /// </summary>
        public const int SERVERS_UNLIMITED = 0;


        /// <summary>
        /// Trial key length.
        /// </summary>
        public const int TRIAL_KEY_LENGTH = 20;


        /// <summary>
        /// Valid product codes for current version.
        /// </summary>
        private static readonly string[] ValidProductCodes = { "CF12", "CB12", "CN12", "CX12", "CV12" };
        
        /// <summary>
        /// If false, the license information is not loaded.
        /// </summary>
        private bool mLicenseLoaded;

        /// <summary>
        /// Salt for the trial license.
        /// </summary>
        private static readonly string E4 = Convert.ToChar(105).ToString() + Convert.ToChar(104) + Convert.ToChar(65) + Convert.ToChar(52) + Convert.ToChar(88) + Convert.ToChar(79) + Convert.ToChar(112) + Convert.ToChar(52);

        /// <summary>
        /// Defines when the all licenses within the system will expire (used for time limited builds).
        /// </summary>
        private DateTime AAC = TIME_UNLIMITED_LICENSE;

        /// <summary>
        /// License version
        /// </summary>
        private string mVersion;

        /// <summary>
        /// Validation result
        /// </summary>
        private LicenseValidationEnum mValidationResult = LicenseValidationEnum.Unknown;

        /// <summary>
        /// Edition type value
        /// </summary>
        private long mEditionValue;

        /// <summary>
        /// Edition type
        /// </summary>
        private ProductEditionEnum mEdition = ProductEditionEnum.Free;
        
        /// <summary>
        /// License servers
        /// </summary>
        private int mLicenseServers = -1;
        
        /// <summary>
        /// Name of license owner.
        /// </summary>
        private string mOwner;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Returns true if key is trial.
        /// </summary>
        public bool IsTrial
        {
            get
            {
                if ((Key != null) && Key.StartsWith(Encoding.ASCII.GetString(Convert.FromBase64String("RE9NQUlOOg==")), StringComparison.Ordinal))
                {
                    return false;
                }

                return true;
            }
        }


        /// <summary>
        /// Edition value.
        /// </summary>        
        public long EditionValue
        {
            get
            {
                if (!mLicenseLoaded)
                {
                    LoadLicense(Key, Domain);
                }
                return mEditionValue;
            }
        }


        /// <summary>
        /// License version.
        /// </summary>
        public string Version
        {
            get
            {
                if (!mLicenseLoaded)
                {
                    LoadLicense(Key, Domain);
                }
                return mVersion;
            }
        }


        /// <summary>
        /// Name of license owner.
        /// </summary>
        public string Owner
        {
            get
            {
                if (!mLicenseLoaded)
                {
                    LoadLicense(Key, Domain);
                }
                return mOwner;
            }
        }


        /// <summary>
        /// License edition.
        /// </summary>
        public ProductEditionEnum Edition
        {
            get
            {
                if (!mLicenseLoaded)
                {
                    LoadLicense(Key, Domain);
                }
                return mEdition;
            }
        }


        /// <summary>
        /// Edition character.
        /// </summary>
        [DatabaseField(ColumnName = "LicenseEdition",ValueType = typeof(char))]
        public char EditionChar
        {
            get
            {
                return Convert.ToChar(GetValue("LicenseEdition"));
            }
        }


        /// <summary>
        /// Expiration date.
        /// </summary>
        [DatabaseField(ColumnName = "LicenseExpiration", ValueType = typeof(string))]
        public DateTime ExpirationDate
        {
            get
            {
                return GetDateTimeValue("LicenseExpiration", TIME_UNLIMITED_LICENSE);
            }
        }


        /// <summary>
        /// Real expiration date.
        /// </summary>
        public DateTime ExpirationDateReal
        {
            get
            {
                if (!mLicenseLoaded)
                {
                    LoadLicense(Key, Domain);
                }
                return AAC;
            }
        }


        /// <summary>
        /// Domain which is assigned to license.
        /// </summary>
        [DatabaseField(ColumnName = "LicenseDomain")]
        public string Domain
        {
            get
            {
                return GetStringValue("LicenseDomain", "");
            }
        }


        /// <summary>
        /// License key.
        /// </summary>
        [DatabaseField(ColumnName = "LicenseKey")]
        public string Key
        {
            get
            {
                return GetStringValue("LicenseKey", "");
            }
        }


        /// <summary>
        /// Gets number of webfarm servers allowed by license.
        /// </summary>
        [DatabaseField]
        public int LicenseServers
        {
            get
            {
                return GetIntegerValue("LicenseServers", 0);
            }
        }


        /// <summary>
        /// ID of license key.
        /// </summary>
        [DatabaseField]
        public int LicenseKeyID
        {
            get
            {
                return GetIntegerValue("LicenseKeyID", 0);
            }
            set
            {
                SetValue("LicenseKeyID", value);
            }
        }


        /// <summary>
        /// Validation result.
        /// </summary>
        public LicenseValidationEnum ValidationResult
        {
            get
            {
                // Load license
                if (!mLicenseLoaded)
                {
                    LoadLicense(Key, Domain);
                }

                if (mValidationResult == LicenseValidationEnum.Unknown)
                {
                    // Get validation result
                    try
                    {
                        mValidationResult = SetDomainInfo(Domain);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        mValidationResult = LicenseValidationEnum.WrongFormat;
                    }
                }

                return mValidationResult;
            }
            set
            {
                switch (value)
                {
                    case LicenseValidationEnum.Invalid:
                    case LicenseValidationEnum.Unknown:
                        mValidationResult = value;
                        break;

                    default:
                        throw new InvalidOperationException("Validation result can be set only to Invalid or Unknown.");
                }
            }
        }

        #endregion

        
        #region "Constructors"

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LicenseKeyInfo()
            : base(TYPEINFO)
        {
            ValidateCodeName = false;
        }


        /// <summary>
        /// Constructor - Creates license key from DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the license data</param>
        public LicenseKeyInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
            ValidateCodeName = false;
        }

        #endregion


        #region "GeneralizedInfo methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            LicenseKeyInfoProvider.DeleteLicenseKeyInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            LicenseKeyInfoProvider.SetLicenseKeyInfo(this);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates product code of license key info.
        /// </summary>
        /// <returns>Product code</returns>
        private string ProductCode()
        {
            return LicenseKeyInfoProvider.EditionToString(Edition) + Version;
        }


        /// <summary>
        /// Validates product code.
        /// </summary>
        /// <param name="productCode">License key info product code</param>
        /// <returns>True if valid, else false</returns>
        private bool ValidateProductCode(string productCode)
        {
            return (Array.IndexOf(ValidProductCodes, productCode) >= 0);
        }


        /// <summary>
        /// Validates expiration date.
        /// </summary>
        /// <returns>True if not expired, else false</returns>
        private bool RemoveDependencies()
        {
            return ((ExpirationDateReal == TIME_UNLIMITED_LICENSE) || (ExpirationDateReal > DateTime.Now));
        }


        /// <summary>
        /// Loads the license.
        /// </summary>
        /// <param name="key">License key</param>
        /// <param name="domain">Domain name</param>
        public void LoadLicense(string key, string domain)
        {
            // If already loaded, return
            if (mLicenseLoaded)
            {
                return;
            }

            if (!String.IsNullOrEmpty(key))
            {
                if (key.Length == TRIAL_KEY_LENGTH)
                {
                    // trial key
                    mEdition = LicenseKeyInfoProvider.StringToEdition(key.Substring(0, 2));
                    mVersion = key.Substring(2, 2);

                    AAC = new DateTime(Convert.ToInt32(key.Substring(5, 4)), Convert.ToInt32(key.Substring(9, 2)), Convert.ToInt32(key.Substring(11, 2)), 0, 0, 0); // real value
                    SetValue("LicenseExpiration", AAC.ToString(LICENSE_EXPIRATION_DATE_FORMAT, CultureInfo.InvariantCulture)); // value for DB and grid

                    if (ValidationHelper.GetString(domain, "") != "")
                    {
                        SetValue("LicenseDomain", domain);
                    }
                    else
                    {
                        // default license domain
                        SetValue("LicenseDomain", "localhost");
                    }
                    SetValue("LicenseKey", key);
                    mEditionValue = (Convert.ToInt32(mEdition) & Domain.ToLowerInvariant()[0] ^ (Convert.ToInt32(mVersion) + 6) & Domain.Length ^ (AAC.Year + 256));

                    // Set unlimited servers (for grid)
                    SetValue("LicenseServers", 0);
                }
                else
                {
                    try
                    {
                        // Full or time limited key
                        var index = key.IndexOf(Encoding.ASCII.GetString(Convert.FromBase64String("RE9NQUlOOg==")), StringComparison.Ordinal) + 7;
                        SetValue("LicenseDomain", key.Substring(index, key.IndexOf("PRODUCT:", StringComparison.Ordinal) - index).Trim().ToLowerInvariant());
                        index = key.IndexOf("PRODUCT:", StringComparison.Ordinal) + 8;
                        var aux = key.Substring(index, key.IndexOf("EXPIRATION:", StringComparison.Ordinal) - index).Trim();
                        mEdition = LicenseKeyInfoProvider.StringToEdition(aux.Substring(0, 2));
                        mVersion = aux.Substring(2, 2);

                        index = key.IndexOf("EXPIRATION:", StringComparison.Ordinal) + 11;
                        aux = key.Substring(index, 8).Trim();
                        if (aux == "00000000")
                        {
                            AAC = TIME_UNLIMITED_LICENSE; // real value
                        }
                        else
                        {
                            AAC = new DateTime(Convert.ToInt32(aux.Substring(0, 4)), Convert.ToInt32(aux.Substring(4, 2)), Convert.ToInt32(aux.Substring(6, 2)), 0, 0, 0); // real value                            
                        }

                        if (key.Contains("OWNER:"))
                        {
                            index = key.IndexOf("OWNER:", StringComparison.Ordinal) + 6;
                            aux = key.Substring(index, key.IndexOf("SERVERS:", StringComparison.Ordinal) - index).Trim();
                            mOwner = aux;
                        }

                        SetValue("LicenseExpiration", AAC.ToString(LICENSE_EXPIRATION_DATE_FORMAT, CultureInfo.InvariantCulture)); // value for DB and grid

                        // Servers
                        index = key.IndexOf("SERVERS:", StringComparison.Ordinal) + 8;
                        aux = key.Substring(index, key.IndexOf("\n", index, StringComparison.Ordinal) - index).Trim();
                        mLicenseServers = ValidationHelper.GetInteger(aux, -1);
                        SetValue("LicenseServers", mLicenseServers);
                        SetValue("LicenseKey", key);
                        mEditionValue = (Convert.ToInt32(mEdition) & Domain.ToLowerInvariant()[0] ^ (Convert.ToInt32(mVersion) + 6) & Domain.Length ^ (AAC.Year + 256));
                    }
                    // Bad loading or unknown product version type
                    catch (Exception)
                    {
                        // Key is not correct, set validation result
                        mValidationResult = LicenseValidationEnum.WrongFormat;
                    }
                }
            }

            mLicenseLoaded = true;
        }


        /// <summary>
        /// Validates license and stores the result into a private variable.
        /// </summary>
        private LicenseValidationEnum SetDomainInfo(string domainName)
        {
            if (Key.StartsWith(Encoding.ASCII.GetString(Convert.FromBase64String("RE9NQUlOOg==")), StringComparison.Ordinal))
            {
                // Validate full key
                // Validate domain name
                if (!String.Equals(Domain, domainName, StringComparison.OrdinalIgnoreCase))
                {
                    return LicenseValidationEnum.Invalid;
                }

                // Validate product code
                if (!ValidateProductCode(ProductCode()))
                {
                    return LicenseValidationEnum.WrongFormat;
                }

                // Validate expiration date
                if (!RemoveDependencies())
                {
                    return LicenseValidationEnum.Expired;
                }

                // Validate RSA signature
                if (!VerifyRSA(Key, Encoding.ASCII.GetString(Convert.FromBase64String("PFJTQUtleVZhbHVlPjxNb2R1bHVzPjBRMUp4NEswTUdLSGZrdDZ0cFpnRlh4MWpxemwvcUtDbVJ1VDVIVFFkSlZaTGpIUFl6b0tXMDZBZ0Z5eU9VblYwRWVzWXdiSkF6QU1ud1NLcGl4dWp3Vnd6VXVxcHE0L2ZkREkweHBWVGN2cEJjaGJqc2VIYVZIL2t5d0dWZ1U0UEFhbDZSRmlmK0pDRkhxYWkxbkZnbVplZUVkY0xJUURzcVRqK1drem5BQnBoZzJxOUJnMWtSeTRRWVdJczBtQ05xZm40dnVaRlFMVjVSeXozUWpTemF0MlB0ZXBQajVvNGxVNndWT2gxR1Y2MW5pTWp4Lzh4blc5N05UMUNIR21KZVJHVWxUY2t2enVHaDJHdzVxL3U5YUYyak41bmFFTTZvZEhmdFhMRVI0L0NESExjdWwyUzZubFlKaHVqbTNnd2dMVmY2TEtBaUVQMjcwVDl5bUIwUT09PC9Nb2R1bHVzPjxFeHBvbmVudD5BUUFCPC9FeHBvbmVudD48L1JTQUtleVZhbHVlPg=="))))
                {
                    return LicenseValidationEnum.WrongFormat;
                }

                return LicenseValidationEnum.Valid;
            }

            // validate trial key
            // trial key is valid only for local domains
            if (!LicenseKeyInfoProvider.IsLocalDomain(domainName))
            {
                return LicenseValidationEnum.Invalid;
            }
            // validate product code
            if (!ValidateProductCode(ProductCode()))
            {
                return LicenseValidationEnum.WrongFormat;
            }
            // validate expiration date
            if (!RemoveDependencies())
            {
                return LicenseValidationEnum.Expired;
            }

            // take signature from the key
            string signature = Key.Substring(14, 6);
            // computes correct license key
            ComputeTrialKey();
            // if given signature and correct signature are not identical
            if (Key.Substring(14, 6) != signature)
            {
                return LicenseValidationEnum.WrongFormat;
            }

            return LicenseValidationEnum.Valid;
        }


        /// <summary>
        /// Computes trial key using SHA1 hash.
        /// </summary>
        private void ComputeTrialKey()
        {
            string edition = LicenseKeyInfoProvider.EditionToString(Edition);
            string key = String.Format(CultureInfo.InvariantCulture, "{0}{1}-{2:yyyyMMdd}-", edition, Version, ExpirationDateReal);

            using (var sha1Provider = new SHA1CryptoServiceProvider())
            {
                var hash = sha1Provider.ComputeHash(Encoding.ASCII.GetBytes(key + "-" + E4));
                key += Convert.ToBase64String(hash).Substring(0, 6);
            }

            SetValue("LicenseKey", key);
        }


        /// <summary>
        /// Verifies RSA signed license key.
        /// </summary>
        /// <param name="inputText">License key</param>
        /// <param name="publicKey">RSA public key</param>
        private static bool VerifyRSA(string inputText, string publicKey)
        {
            try
            {
                // Ensure the correct line endings
                if (!inputText.Contains("\r\n"))
                {
                    inputText = TextHelper.EnsureLineEndings(inputText, "\r\n");
                }

                using (RSACryptoServiceProvider myRsaProvider = new RSACryptoServiceProvider())
                {
                    // Load public key
                    myRsaProvider.FromXmlString(publicKey);

                    // Get input key
                    string inputKey = inputText.Substring(0, inputText.IndexOf("\r\n", inputText.IndexOf("SERVERS:", StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase));

                    // Add end of line in default
                    if (!inputKey.EndsWith("\r\n", StringComparison.OrdinalIgnoreCase))
                    {
                        inputKey += "\r\n";
                    }

                    // Get byte array of input key
                    byte[] inputKeyBytes = Encoding.ASCII.GetBytes(inputKey);

                    // Get input signature
                    string inputSignature = inputText.Substring(inputText.IndexOf("\r\n", inputText.IndexOf("SERVERS:", StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase) + 2);

                    // Get value from base64
                    byte[] inputSignatureBytes = Convert.FromBase64String(inputSignature);

                    //Verify key
                    using (SHA1CryptoServiceProvider sha1CSP = new SHA1CryptoServiceProvider())
                    {
                        return myRsaProvider.VerifyData(inputKeyBytes, sha1CSP, inputSignatureBytes);
                    }
                }
            }
            catch
            {
                // if something is wrong, then license key is not valid
                return false;
            }
        }

        #endregion
    }
}