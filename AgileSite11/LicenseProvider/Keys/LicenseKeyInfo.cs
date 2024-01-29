using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
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
        private static readonly string[] validProductCodes = { "CF11", "CB11", "CN11", "CX11", "CV11" };
        
        /// <summary>
        /// If false, the license information is not loaded.
        /// </summary>
        public bool mLicenseLoaded = false;

        /// <summary>
        /// Salt for the trial license.
        /// </summary>
        private static readonly string E4 = Convert.ToChar(84).ToString() + Convert.ToChar(118) + Convert.ToChar(70) + Convert.ToChar(117) + Convert.ToChar(115) + Convert.ToChar(56) + Convert.ToChar(111) + Convert.ToChar(69);

        /// <summary>
        /// Defines when the all licenses within the system will expire (used for time limited builds).
        /// </summary>
        private DateTime AAC = TIME_UNLIMITED_LICENSE;

        /// <summary>
        /// License version
        /// </summary>
        private string oVersion;

        /// <summary>
        /// Validation result
        /// </summary>
        private LicenseValidationEnum iValidationResult = LicenseValidationEnum.Unknown;

        /// <summary>
        /// Edition type value
        /// </summary>
        private long cEditionValue = 0;

        /// <summary>
        /// Edition type
        /// </summary>
        private ProductEditionEnum mEdition = ProductEditionEnum.Free;
        
        /// <summary>
        /// License servers
        /// </summary>
        private int mLicenseServers = -1;

        /// <summary>
        /// Contains license custom modules - currently disabled functionality.
        /// </summary>
        private Hashtable F0 = null;
        
        /// <summary>
        /// Name of license owner.
        /// </summary>
        private string mOwner = null;

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
                return cEditionValue;
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
                return oVersion;
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
        /// Gets number of servers supported by the license.
        /// </summary>
        private int Servers
        {
            get
            {
                // Load license if necessary
                if (!mLicenseLoaded)
                {
                    LoadLicense(Key, Domain);
                }
                return mLicenseServers;
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

                if (iValidationResult == LicenseValidationEnum.Unknown)
                {
                    // Get validation result
                    try
                    {
                        iValidationResult = SetDomainInfo(Domain);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        iValidationResult = LicenseValidationEnum.WrongFormat;
                    }
                }

                return iValidationResult;
            }
            set
            {
                switch (value)
                {
                    case LicenseValidationEnum.Invalid:
                    case LicenseValidationEnum.Unknown:
                        iValidationResult = value;
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
        /// Load custom modules.
        /// </summary>
        private void LoadCustoModules(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                // Initialize hashtable
                F0 = new Hashtable();

                // Load module list
                Hashtable modulesArray = LicenseKeyInfoProvider.GetModules();

                // Split modules in key
                string[] modules = key.Split('|');

                // Check every custom module
                for (int i = 0; i < modules.Length; i++)
                {
                    if (modulesArray[modules[i]] != null)
                    {
                        // Set module Feature Enum to the modules hashtable
                        F0[Enum.Parse(typeof(FeatureEnum), modulesArray[modules[i]].ToString())] = 0;
                    }
                }
            }
        }


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
            return (Array.IndexOf(validProductCodes, productCode) >= 0);
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
                    oVersion = key.Substring(2, 2);

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
                    cEditionValue = (Convert.ToInt32(mEdition) & Domain.ToLowerInvariant()[0] ^ (Convert.ToInt32(oVersion) + 6) & Domain.Length ^ (AAC.Year + 256));

                    // Set unlimited servers (for grid)
                    SetValue("LicenseServers", 0);
                }
                else
                {
                    try
                    {
                        // Full or time limited key
                        string aux = "";
                        int index = 0;

                        index = key.IndexOf(Encoding.ASCII.GetString(Convert.FromBase64String("RE9NQUlOOg==")), StringComparison.Ordinal) + 7;
                        SetValue("LicenseDomain", key.Substring(index, key.IndexOf("PRODUCT:", StringComparison.Ordinal) - index).Trim().ToLowerInvariant());
                        index = key.IndexOf("PRODUCT:", StringComparison.Ordinal) + 8;
                        aux = key.Substring(index, key.IndexOf("EXPIRATION:", StringComparison.Ordinal) - index).Trim();
                        mEdition = LicenseKeyInfoProvider.StringToEdition(aux.Substring(0, 2));
                        oVersion = aux.Substring(2, 2);

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


                        if (key.Contains("CUSTOM:"))
                        {
                            index = key.IndexOf("CUSTOM:", StringComparison.Ordinal) + 7;
                            string tmpCustom = key.Remove(0, index);
                            tmpCustom = tmpCustom.Remove(tmpCustom.IndexOf("\r\n", StringComparison.Ordinal));
                            LoadCustoModules(tmpCustom);
                        }

                        SetValue("LicenseKey", key);
                        cEditionValue = (Convert.ToInt32(mEdition) & Domain.ToLowerInvariant()[0] ^ (Convert.ToInt32(oVersion) + 6) & Domain.Length ^ (AAC.Year + 256));
                    }
                    // Bad loading or unknown product version type
                    catch (Exception)
                    {
                        // Key is not correct, set validation result
                        iValidationResult = LicenseValidationEnum.WrongFormat;
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
                if (Domain.ToLowerCSafe() != domainName.ToLowerCSafe())
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
                if (!EncryptionHelper.VerifyRSA(Key, Encoding.ASCII.GetString(Convert.FromBase64String("PFJTQUtleVZhbHVlPjxNb2R1bHVzPnJmbjYzSzhsNnF0dU1pOUtNN2MxUzMyNFE1ejQ0bXZkeGw4M0tYdVBOSWJRV2k3TWxBT1d2RTVhMzdsTXZHRWl2R1ZwMDd5aFc3QTlyWS9lMFBNQTZkNFJPWDdzWGxIdTltem9hTjBQVGh2bWU0T2dGZVp5VTVwZzlhK0VlenJlWVQ4RkJvRHdESkU2ckp2TkwrbCtlbWJzdG9zdnlaYmtNR2R0dDBCaVF2UHJMVUVkaWdBZVJVNndJVnhpTTBNamRNZ1pZS042Z3dSQWRLT2d2OXZBcGpwbmNiQUp6K2dsSlFKNko1RDJ4dlNFVUw3czlzVGpNaG80Mm4wRlhmOHdpODlzV1phYXVkd2lHN293WUxpZkNmT1NTOURaMFFVOU96MDB1eFlZQmtJdWJ1SzdkVHBQNkN0WXJVbEx4SFBDK2Z6MXRMK3hiZnZTVTJNalBVSEZmUT09PC9Nb2R1bHVzPjxFeHBvbmVudD5BUUFCPC9FeHBvbmVudD48L1JTQUtleVZhbHVlPg=="))))
                {
                    return LicenseValidationEnum.WrongFormat;
                }

                return LicenseValidationEnum.Valid;
            }
            else
            {
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

        #endregion
    }
}