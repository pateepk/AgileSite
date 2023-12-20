using System;
using System.Collections;
using System.Linq;
using System.Text;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.LicenseProvider
{
    /// <summary>
    /// Class providing LicenseKeyInfo management.
    /// </summary>
    public class LicenseKeyInfoProvider : AbstractInfoProvider<LicenseKeyInfo, LicenseKeyInfoProvider>
    {
        #region "Variables"

        // Table of license keys indexed by domain name
        private static readonly CMSStatic<StringSafeDictionary<LicenseKeyInfo>> mB = new CMSStatic<StringSafeDictionary<LicenseKeyInfo>>(() => new StringSafeDictionary<LicenseKeyInfo>());

        // Table of available features indexed by domain name and feature
        private static readonly CMSStatic<StringSafeDictionary<bool?>> mC = new CMSStatic<StringSafeDictionary<bool?>>(() => new StringSafeDictionary<bool?>());

        // Indicates whether only trial licenses are available
        private static readonly CMSStatic<bool?> mOnlyTrialLicenses = new CMSStatic<bool?>();

        // Validation of license for specific edition
        private static readonly CMSStatic<ProviderDictionary<string, bool>> mJ2656 = new CMSStatic<ProviderDictionary<string, bool>>(GetLicenseDictionary);

        // Random for license calculation
        private static readonly Random rnd = new Random();

        // Indicates whether license keys are loaded or not.
        private static readonly CMSStatic<bool> mIsLoaded = new CMSStatic<bool>();

        private static string allowedExtraDomain = string.Empty;
        private static readonly object lockObject = new object();

        /// <summary>
        /// Codename which identifies time unlimited license
        /// </summary>
        public const string TIME_UNLIMITED_LICENSE_CODENAME = "LicenseInfoProvider.FullLicense";

        #endregion


        #region "Properties"

        /// <summary>
        /// Table of license keys indexed by domain name
        /// </summary>
        private static StringSafeDictionary<LicenseKeyInfo> B
        {
            get
            {
                return mB;
            }
            set
            {
                mB.Value = value;
            }
        }


        /// <summary>
        /// Table of available features indexed by domain name and feature
        /// </summary>
        private static StringSafeDictionary<bool?> C
        {
            get
            {
                return mC;
            }
        }


        /// <summary>
        /// Validation of license for specific edition
        /// </summary>
        private static ProviderDictionary<string, bool> J2656
        {
            get
            {
                return mJ2656;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets an empty dictionary for licenses
        /// </summary>
        private static ProviderDictionary<string, bool> GetLicenseDictionary()
        {
            return new ProviderDictionary<string, bool>("CMS.License|EditionExists", null, StringComparer.InvariantCultureIgnoreCase, true);
        }


        /// <summary>
        /// Returns the LicenseKeyInfo structure for the specified licenseKey.
        /// </summary>
        /// <param name="licenseKeyId">LicenseKey id</param>
        public static LicenseKeyInfo GetLicenseKeyInfo(int licenseKeyId)
        {
            return GetLicenseKeys().WhereEquals("LicenseKeyID", licenseKeyId).TopN(1).FirstOrDefault();
        }


        /// <summary>
        /// Parses domain name from URL.
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>Domain name</returns>
        public static string ParseDomainName(string url)
        {
            if (String .IsNullOrEmpty(url))
            {
                return String.Empty;
            }

            // Index of: http(s)://
            var schemeIndex = url.IndexOf("://", StringComparison.Ordinal);
            int startIndex = 0;
            if (schemeIndex >= 0)
            {
                startIndex = schemeIndex + 3;
            }

            // Index of: http(s)://www.
            var wwwIndex = url.IndexOf("www.", startIndex, 4, StringComparison.OrdinalIgnoreCase);
            if (wwwIndex >= 0)
            {
                startIndex += 4;
            }

            // Index of domain path separator: ..domain/pathAndQuery
            var pathIndex = url.IndexOf("/", startIndex, StringComparison.Ordinal);
            int endIndex = url.Length - startIndex;
            if (pathIndex >= 0)
            {
                endIndex = pathIndex - startIndex;
            }

            url = url.Substring(startIndex, endIndex);

            return URLHelper.RemovePort(url);
        }


        /// <summary>
        /// Returns the LicenseKeyInfo structure for the specified domain.
        /// </summary>
        /// <param name="domain">LicenseDomain</param>
        public static LicenseKeyInfo GetLicenseKeyInfo(string domain)
        {
            LicenseKeyInfo result = null;
            if (domain != null)
            {
                domain = URLHelper.RemoveWWW(domain.ToLowerInvariant());
                domain = URLHelper.RemovePort(domain);

                // Try to get license from hashtable
                result = B[domain];

                // If not found in hashtable, try to find it in DB
                if (result == null)
                {
                    // Try to get a domain specific license first
                    if (!IsLocalDomain(domain))
                    {
                        result = GetLicenseKeyInfoFromDB(domain);
                        lock (lockObject)
                        {
                            B[domain] = result;
                        }
                    }
                }

                // If license not found
                if (result == null)
                {
                    bool allowedExtra = AzureHelper.IsAzureStagingDomain(domain);

                    // localhost domains and extra domains - these are valid for all non-expired license keys
                    if (allowedExtra || IsLocalDomain(domain))
                    {
                        // Save extra domain
                        if (allowedExtra)
                        {
                            allowedExtraDomain = domain;
                        }

                        result = GetBestLicense();

                        lock (lockObject)
                        {
                            Hashtable licenses = B;

                            // Save localhost license result to the hashtable
                            if (result != null)
                            {
                                licenses["localhost"] = result;

                                // Save license for extra domain
                                if (!string.IsNullOrEmpty(allowedExtraDomain))
                                {
                                    licenses[allowedExtraDomain] = result;
                                }
                            }
                        }
                    }
                }

                if (result != null)
                {
                    // Check the license values validity
                    if (rnd.Next(0, 5000) == 665)
                    {
                        try
                        {
                            ProductEditionEnum resultEdition = result.Edition;
                            DateTime resultExpiration = result.ExpirationDateReal;
                            LicenseValidationEnum resultValidationResult = result.ValidationResult;

                            string resultVersion = result.Version;

                            // Refresh license
                            result.LoadLicense(result.Key, result.Domain);

                            // Check if the result value match
                            if ((result.Edition != resultEdition) || (result.ExpirationDateReal != resultExpiration) || (result.ValidationResult != resultValidationResult) || (result.Version != resultVersion))
                            {
                                throw new InvalidOperationException(Encoding.ASCII.GetString(Convert.FromBase64String("IUltcG9ydGFudCAtIFtDTVMuQ29yZV0gOiBTeW5jaHJvbml6YXRpb24gZmFpbGVkISEhIFBsZWFzZSBjb250YWN0IHVzIGFzIHNvb24gYXMgcG9zc2libGUu")));
                            }
                        }
                        catch
                        {
                            throw new InvalidOperationException(Encoding.ASCII.GetString(Convert.FromBase64String("IUltcG9ydGFudCAtIFtDTVMuQ29yZV0gOiBTeW5jaHJvbml6YXRpb24gZmFpbGVkISEhIFBsZWFzZSBjb250YWN0IHVzIGFzIHNvb24gYXMgcG9zc2libGUu")));
                        }

                        try
                        {
                            if (!String.IsNullOrEmpty(result.Domain) && (result.EditionValue != (Convert.ToInt32(result.Edition) & result.Domain.ToLowerInvariant()[0] ^ (Convert.ToInt32(result.Version) + 6) & result.Domain.Length ^ (result.ExpirationDateReal.Year + 256))))
                            {
                                result.ValidationResult = LicenseValidationEnum.Invalid;
                                LicenseHelper.Clear();
                            }
                        }
                        catch
                        {
                            throw new InvalidOperationException(Encoding.ASCII.GetString(Convert.FromBase64String("TGljZW5zZSBrZXkgaXMgY29ycnVwdGVkLg==")));
                        }
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Returns the best available license for the given feature.
        /// </summary>
        public static LicenseKeyInfo GetBestLicense()
        {
            LoadKeys();

            LicenseKeyInfo result = null;

            lock (lockObject)
            {
                Hashtable licenses = B;

                // Get the best license available
                foreach (LicenseKeyInfo license in licenses.Values)
                {
                    if ((license == null) || (license.ValidationResult != LicenseValidationEnum.Valid))
                    {
                        continue;
                    }

                    // Compare licenses
                    if (!IsBetterLicense(license, result))
                    {
                        continue;
                    }

                    result = license;
                    if (result.Edition == ProductEditionEnum.EnterpriseMarketingSolution)
                    {
                        // EMS is the best, no better license can be found
                        break;
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Returns true if first license is better than the other for specified feature.
        /// </summary>
        /// <param name="license1">First license</param>
        /// <param name="license2">Second license</param>
        public static bool IsBetterLicense(LicenseKeyInfo license1, LicenseKeyInfo license2)
        {
            if ((license2 == null) || (license2.ValidationResult != LicenseValidationEnum.Valid))
            {
                // No license yet, first is better
                return true;
            }

            if ((license1 != null) && (license1.ValidationResult == LicenseValidationEnum.Valid))
            {
                if (license2.Edition < license1.Edition)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns LicenseKeyInfo object for specified domain.
        /// </summary>
        /// <param name="domain">License domain</param>
        private static LicenseKeyInfo GetLicenseKeyInfoFromDB(string domain)
        {
            if (String.IsNullOrEmpty(domain))
            {
                return null;
            }

            // License domain preserved in DB is expected to be in non-shortened format (atleast the generator creates it this way).
            domain = LicenseAddressService.Instance.ToFullFormat(domain);
            return GetLicenseKeys().WhereEquals("LicenseDomain", domain).TopN(1).FirstOrDefault();
        }


        /// <summary>
        /// Returns the query for all licenses.
        /// </summary>
        public static ObjectQuery<LicenseKeyInfo> GetLicenseKeys()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns true if exists at least one valid license for specific edition
        /// </summary>
        /// <param name="edition">Product edition enum</param>
        public static bool LicenseEditionExists(ProductEditionEnum edition)
        {
            // Get edition char
            string editChar = edition.ToStringRepresentation();

            // Check whether exists cached value
            if (!J2656.ContainsKey(editChar))
            {
                // Edition status flag
                bool editionStatusSet = false;

                var licenses = GetLicenseKeys().WhereEquals("LicenseEdition", editChar);

                // Loop thru all licenses
                foreach (var license in licenses)
                {
                    try
                    {
                        // Cache result if license is valid
                        if (license.ValidationResult == LicenseValidationEnum.Valid)
                        {
                            J2656[editChar] = true;
                            editionStatusSet = true;
                            break;
                        }
                    }
                    catch
                    {
                        // License is not valid
                    }
                }

                // Set false value if validation was not set
                if (!editionStatusSet)
                {
                    J2656[editChar] = false;
                }
            }

            return ValidationHelper.GetBoolean(J2656[editChar], false);
        }


        /// <summary>
        /// Sets specified licenseKey.
        /// </summary>
        /// <param name="licenseKey">LicenseKey to set</param>
        public static void SetLicenseKeyInfo(LicenseKeyInfo licenseKey)
        {
            if (licenseKey == null)
            {
                throw new ArgumentNullException(nameof(licenseKey), Encoding.ASCII.GetString(Convert.FromBase64String("Tm8gTGljZW5zZUtleUluZm8gb2JqZWN0IHNldC4=")));
            }

            // Get existing
            if (IsLicenseExistForDomain(licenseKey))
            {
                throw new InvalidOperationException(Encoding.ASCII.GetString(Convert.FromBase64String("TGljZW5zZSBmb3IgZG9tYWluIGFscmVhZHkgZXhpc3RzLg==")));
            }

            var expiration = (licenseKey.ExpirationDateReal == LicenseKeyInfo.TIME_UNLIMITED_LICENSE) ? TIME_UNLIMITED_LICENSE_CODENAME : licenseKey.ExpirationDateReal.ToString(LicenseKeyInfo.LICENSE_EXPIRATION_DATE_FORMAT);

            licenseKey.SetValue("LicenseExpiration", expiration);
            licenseKey.SetValue("LicenseEdition", licenseKey.Edition.ToStringRepresentation());

            using (new CMSActionContext { CheckLicense = false })
            {
                ProviderObject.SetInfo(licenseKey);
            }

            // Add license to hashtable
            lock (lockObject)
            {
                var domain = licenseKey.Domain;

                B[ParseDomainName(domain)] = licenseKey;

                if (IsLocalDomain(domain))
                {
                    // Set license for the localhost domain
                    B["localhost"] = licenseKey;
                }
                else
                {
                    // Clear localhost licenses
                    B["localhost"] = null;
                }
            }

            // Clear the hashtables
            ClearHashtables(licenseKey);
        }


        /// <summary>
        /// Deletes specified license key.
        /// </summary>
        /// <param name="licenseKey">License key object</param>
        public static void DeleteLicenseKeyInfo(LicenseKeyInfo licenseKey)
        {
            if (licenseKey == null)
            {
                return;
            }

            // Delete the object
            ProviderObject.DeleteInfo(licenseKey);

            // The site can still run under localhost domain if license for localhost exists
            var localhostLicense = GetLicenseKeyInfoFromDB("localhost");

            lock (lockObject)
            {
                B[licenseKey.Domain] = null;

                // Updates localhost license
                B["localhost"] = localhostLicense;

                // Clear extra domain license
                if (B.Contains(allowedExtraDomain))
                {
                    B[allowedExtraDomain] = null;
                }
            }

            // Clear the hashtables
            ClearHashtables(licenseKey);
        }


        /// <summary>
        /// Clears the hashtables based on the given license key
        /// </summary>
        /// <param name="lki">License key</param>
        private static void ClearHashtables(LicenseKeyInfo lki)
        {
            ProviderObject.CreateWebFarmTask("licensekeyinfoprovider", lki.Domain);

            // Clear checked urls hashtable
            LicenseHelper.Clear();

            C.Clear();

            mOnlyTrialLicenses.Value = null;
            J2656.Delete(Convert.ToString(lki.EditionChar));
        }


        /// <summary>
        /// Deletes specified licenseKey.
        /// </summary>
        /// <param name="licenseKeyId">LicenseKey id</param>
        public static void DeleteLicenseKeyInfo(int licenseKeyId)
        {
            LicenseKeyInfo lki = GetLicenseKeyInfo(licenseKeyId);
            DeleteLicenseKeyInfo(lki);
        }


        /// <summary>
        /// Loads all keys from the database into LicenseKeys hashtable.
        /// </summary>
        private static void LoadKeys()
        {
            if (mIsLoaded)
            {
                return;
            }

            var ht = new StringSafeDictionary<LicenseKeyInfo>();

            // Get the data
            var licenses = GetLicenseKeys();

            // Fill in the HashTable
            foreach (var license in licenses)
            {
                ht[license.Domain] = license;
            }

            B = ht;

            mIsLoaded.Value = true;
        }


        /// <summary>
        /// Returns true if feature is somewhat enabled(version limitations) in all editions.
        /// </summary>
        /// <param name="feature">Feature to check</param>
        private static bool IsCommonFeature(FeatureEnum feature)
        {
            switch (feature)
            {
                // Version limitation provide these modules - new in 3.0 version
                case FeatureEnum.BizForms:
                case FeatureEnum.Polls:
                case FeatureEnum.Forums:
                case FeatureEnum.Newsletters:
                case FeatureEnum.Subscribers:
                case FeatureEnum.Blogs:
                case FeatureEnum.Ecommerce:
                case FeatureEnum.SiteMembers:
                case FeatureEnum.CustomTables:
                // New in version 10.0
                case FeatureEnum.SimpleContactManagement:
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Checks if specified feature is available for specified edition.
        /// </summary>
        /// <param name="edition">Product edition</param>
        /// <param name="feature">Feature</param>
        private static bool IsFeatureAvailable(ProductEditionEnum edition, FeatureEnum feature)
        {
            // Check if it is common feature
            if ((CMSApplication.ApplicationInitialized != true) || !CMSActionContext.CurrentCheckLicense || IsCommonFeature(feature))
            {
                return true;
            }

            // Check feature based on edition
            switch (edition)
            {
                #region "Free edition - White list"

                case ProductEditionEnum.Free:
                    {
                        switch (feature)
                        {
                            case FeatureEnum.Webfarm:
                                return true;
                        }
                    }
                    return false;

                #endregion


                #region "SmallBusiness edition - White list"

                case ProductEditionEnum.SmallBusiness:
                    {
                        switch (feature)
                        {
                            case FeatureEnum.WorkflowVersioning: // Under version limitation from 5.0 version
                            case FeatureEnum.Multilingual: // Under version limitation from 5.0 version
                            case FeatureEnum.DocumentLevelPermissions:
                            case FeatureEnum.Membership:
                            case FeatureEnum.ObjectVersioning:
                            case FeatureEnum.NewsletterTracking:
                                return true;
                        }
                    }
                    break;

                #endregion


                #region "Base edition - White list"

                case ProductEditionEnum.Base:
                    {
                        switch (feature)
                        {
                            case FeatureEnum.WorkflowVersioning: // Under version limitation from 5.0 version
                            case FeatureEnum.Multilingual: // Under version limitation from 5.0 version
                            case FeatureEnum.DocumentLevelPermissions:
                            case FeatureEnum.EventManager:
                            case FeatureEnum.Webfarm:
                            case FeatureEnum.Membership:
                            case FeatureEnum.ObjectVersioning:
                            case FeatureEnum.NewsletterTracking:
                                return true;
                        }
                    }
                    break;

                #endregion


                #region "EMS edition - All"

                case ProductEditionEnum.EnterpriseMarketingSolution:
                    // EMS edition contains all features
                    return true;

                #endregion


                #region "Ultimate edition version 7 and higher - Black list"

                case ProductEditionEnum.UltimateV7:

                    // Check unavailable features
                    switch (feature)
                    {
                        case FeatureEnum.ContentPersonalization:
                        // ContactManagement was renamed to FullContactManagement in version 10.0
                        case FeatureEnum.FullContactManagement:
                        case FeatureEnum.LeadScoring:
                        case FeatureEnum.HealthMonitoring:
                        case FeatureEnum.MultipleSMTPServers:
                        case FeatureEnum.SchedulerWinService:
                        case FeatureEnum.SalesForce:
                        case FeatureEnum.AdvancedWorkflow:
                        case FeatureEnum.MarketingAutomation:
                        case FeatureEnum.DBSeparation:
                        case FeatureEnum.NewsletterABTesting:
                        // AB and MVT were removed from ultimate license in version 7
                        case FeatureEnum.ABTesting:
                        case FeatureEnum.MVTesting:
                        case FeatureEnum.CampaignAndConversions:
                        case FeatureEnum.TranslationServices:
                        case FeatureEnum.SocialMarketingInsights:
                        case FeatureEnum.Personas:
                        case FeatureEnum.DataProtection:
                            return false;

                        default:
                            return true;
                    }

                    #endregion
            }

            return false;
        }


        /// <summary>
        /// Checks if specified feature is available for specified domain.
        /// </summary>
        /// <param name="lki">License key</param>
        /// <param name="feature">Feature</param>
        public static bool IsFeatureAvailable(LicenseKeyInfo lki, FeatureEnum feature)
        {
            // No license, no features
            if (lki == null)
            {
                return false;
            }

            // Check the application validity
            if (!LicenseHelper.CheckApplicationValidity())
            {
                return false;
            }

            var result = IsFeatureAvailable(lki.Edition, feature);

            // Log license check
            SecurityDebug.LogSecurityOperation(null, "IsFeatureAvailable", null, feature.ToString(), result, lki.Domain);

            return result;
        }


        /// <summary>
        /// Checks if specified feature is available for specified domain.
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="feature">Feature type</param>
        public static bool IsFeatureAvailable(string domain, FeatureEnum feature)
        {
            // If the application is not initialized, do not check license
            if ((CMSApplication.ApplicationInitialized != true) || !CMSActionContext.CurrentCheckLicense)
            {
                return true;
            }

            // Prepare the key
            string key = $"{domain}|{feature}";

            if (!C.TryGetValue(key, out var result))
            {
                // Get license key and check feature
                LicenseKeyInfo lki = GetLicenseKeyInfo(domain);

                result = (lki != null) && IsFeatureAvailable(lki, feature);

                C[key] = result;
            }

            return result.Value;
        }


        /// <summary>
        /// Checks if feature is available for current domain.
        /// </summary>
        /// <param name="feature">Feature to check</param>
        public static bool IsFeatureAvailable(FeatureEnum feature)
        {
            return IsFeatureAvailable(LicenseHelper.CurrentEdition, feature);
        }


        /// <summary>
        /// Returns specified number for selected feature.
        /// </summary>
        /// <param name="lki">License key</param>
        /// <param name="feature">Feature</param>
        public static int VersionLimitations(LicenseKeyInfo lki, FeatureEnum feature)
        {
            // If the application is not initialized
            if (CMSApplication.ApplicationInitialized != true)
            {
                return LicenseHelper.LIMITATIONS_UNLIMITED;
            }

            // Check the application validity
            if (!LicenseHelper.CheckApplicationValidity())
            {
                return LicenseHelper.LIMITATIONS_NOITEMS;
            }

            if (lki != null)
            {
                switch (lki.Edition)
                {
                    #region "EMS license"

                    // EMS
                    case ProductEditionEnum.EnterpriseMarketingSolution:

                        return LicenseHelper.LIMITATIONS_UNLIMITED;

                    #endregion

                    #region "Ultimate license"

                    case ProductEditionEnum.UltimateV7:
                        switch (feature)
                        {
                            // Newsletters
                            case FeatureEnum.Newsletters:
                                return 10;

                            // Contacts (former subscribers)
                            case FeatureEnum.SimpleContactManagement:
                                return 5000;
                        }
                        break;

                    #endregion

                    #region "Small business license"

                    case ProductEditionEnum.SmallBusiness:
                        switch (feature)
                        {
                            // Editors
                            case FeatureEnum.Editors:
                                return 3;

                            // Newsletters
                            case FeatureEnum.Newsletters:
                                return 1;

                            // Contacts (former subscribers)
                            case FeatureEnum.SimpleContactManagement:
                                return 500;

                            // BizForm
                            case FeatureEnum.BizForms:
                                return 10;

                            // Custom tables
                            case FeatureEnum.CustomTables:
                                return 5;

                            // Forums
                            case FeatureEnum.Forums:
                                return 3;

                            // Blogs
                            case FeatureEnum.Blogs:
                                return 5;

                            // Ecommerce
                            case FeatureEnum.Ecommerce:
                                return 100;

                            // Multiple languages
                            case FeatureEnum.Multilingual:
                                return 2;

                            // Workflow
                            case FeatureEnum.WorkflowVersioning:
                                return LicenseHelper.LIMITATIONS_BASICWORKFLOW;
                        }
                        break;

                    #endregion


                    #region "Base license"

                    case ProductEditionEnum.Base:
                        switch (feature)
                        {
                            // Blogs
                            case FeatureEnum.Blogs:
                                return 5;

                            // Newsletters
                            case FeatureEnum.Newsletters:
                                return 5;

                            // Contacts (former subscribers)
                            case FeatureEnum.SimpleContactManagement:
                                return 500;

                            // Ecommerce
                            case FeatureEnum.Ecommerce:
                                return 500;
                        }
                        break;

                    #endregion


                    #region "Free license"

                    // Free
                    case ProductEditionEnum.Free:
                        switch (feature)
                        {
                            // Global administrators
                            case FeatureEnum.Administrators:
                                return 1;

                            // Editors
                            case FeatureEnum.Editors:
                                return 1;

                            // Documents
                            case FeatureEnum.Documents:
                                return 1000;

                            //Site members
                            case FeatureEnum.SiteMembers:
                                return 100;

                            // Newsletters
                            case FeatureEnum.Newsletters:
                                return 1;

                            // Contacts (former subscribers)
                            case FeatureEnum.SimpleContactManagement:
                                return 100;

                            // BizForm
                            case FeatureEnum.BizForms:
                                return 1;

                            // Forums
                            case FeatureEnum.Forums:
                                return 3;

                            // Blogs
                            case FeatureEnum.Blogs:
                                return 1;

                            // Polls
                            case FeatureEnum.Polls:
                                return 1;

                            // Ecommerce
                            case FeatureEnum.Ecommerce:
                                return 10;

                            // Custom tables
                            case FeatureEnum.CustomTables:
                                return 1;

                            // Multiple languages - not allowed in this edition, but one by default
                            case FeatureEnum.Multilingual:
                                return 1;

                            // Workflow
                            case FeatureEnum.WorkflowVersioning:
                                return LicenseHelper.LIMITATIONS_NOITEMS;

                            // Restrictive behavior for free edition, what is not allowed above is forbidden
                            default:
                                return LicenseHelper.LIMITATIONS_NOITEMS;
                        }

                        #endregion
                }
            }

            return LicenseHelper.LIMITATIONS_UNLIMITED;
        }


        /// <summary>
        /// Returns specified number for selected feature.
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="feature">Feature type</param>
        /// <param name="siteCheck">If true limitations are not applied under URLs in Admin, CMSModules and CMSPages/Logon</param>
        public static int VersionLimitations(string domain, FeatureEnum feature, bool siteCheck = true)
        {
            // If the application is not initialized
            if ((CMSApplication.ApplicationInitialized != true) || !CMSActionContext.CurrentCheckLicense)
            {
                return LicenseHelper.LIMITATIONS_UNLIMITED;
            }

            domain = URLHelper.RemoveWWW(domain);

            var lki = GetLicenseKeyInfo(domain);
            if (lki != null)
            {
                if (siteCheck)
                {
                    var url = CMSHttpContext.Current?.Request?.AppRelativeCurrentExecutionFilePath;
                    if (url != null)
                    {
                        url = url.Remove(0, 2); // remove ~/

                        if (url.StartsWith("cmspages/logon", StringComparison.OrdinalIgnoreCase)
                            || url.StartsWith("cmsmodules/licenses", StringComparison.OrdinalIgnoreCase)
                            || url.StartsWith("cmsmodules/system", StringComparison.OrdinalIgnoreCase)
                            || url.StartsWith("admin/", StringComparison.OrdinalIgnoreCase))
                        {
                            return LicenseHelper.LIMITATIONS_UNLIMITED;
                        }

                        // CMSModules folder is not excluded if feature Editors or Global admins
                        if (url.StartsWith("cmsmodules/", StringComparison.OrdinalIgnoreCase) && (feature != FeatureEnum.Editors) && (feature != FeatureEnum.Administrators))
                        {
                            return LicenseHelper.LIMITATIONS_UNLIMITED;
                        }
                    }
                }

                return VersionLimitations(lki, feature);
            }

            return 0;
        }


        /// <summary>
        /// Clear hashtables.
        /// </summary>
        internal static void Clear()
        {
            lock (lockObject)
            {
                B.Clear();
                C.Clear();

                mIsLoaded.Value = false;
            }
        }


        /// <summary>
        /// Removes domain from cached collection of licencese
        /// </summary>
        internal static void RemoveFromStaticCollection(string domain)
        {
            if (B != null)
            {
                lock (lockObject)
                {
                    B[domain] = null;

                    // License keys need to be reloaded
                    mIsLoaded.Value = false;
                }
            }
        }


        /// <summary>
        /// Checks if license key exist for specified domain.
        /// </summary>
        /// <param name="licenseKey">License key info with specified domain</param>
        public static bool IsLicenseExistForDomain(LicenseKeyInfo licenseKey)
        {
            if ((licenseKey != null) && (!string.IsNullOrEmpty(licenseKey.Domain)))
            {
                // Get existing
                var existing = IsLocalDomain(licenseKey.Domain)
                    ? GetLicenseKeyInfoFromDB(licenseKey.Domain)
                    : GetLicenseKeyInfo(licenseKey.Domain);

                if ((existing != null) && (existing.LicenseKeyID != licenseKey.LicenseKeyID))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Check if given domain is localhost domain.
        /// </summary>
        /// <param name="domain">Domain to check.</param>
        public static bool IsLocalDomain(string domain)
        {
            return LicenseAddressService.Instance.IsLocal(domain);
        }

        #endregion


        #region "Conversion methods"

        /// <summary>
        /// Converts ProductEditionEnum to string.
        /// </summary>
        /// <param name="edition">Edition of the product</param>
        public static string EditionToString(ProductEditionEnum edition)
        {
            switch (edition)
            {
                case ProductEditionEnum.Free:
                    return "CF";
                case ProductEditionEnum.Base:
                    return "CB";
                case ProductEditionEnum.UltimateV7:
                    return "CV";
                case ProductEditionEnum.SmallBusiness:
                    return "CN";
                case ProductEditionEnum.EnterpriseMarketingSolution:
                    return "CX";
                default:
                    return "";
            }
        }


        /// <summary>
        /// Converts string to ProductEditionEnum.
        /// </summary>
        /// <param name="data">String</param>
        public static ProductEditionEnum StringToEdition(string data)
        {
            switch (data)
            {
                case "CF":
                    return ProductEditionEnum.Free;

                case "CB":
                    return ProductEditionEnum.Base;

                case "CV":
                    return ProductEditionEnum.UltimateV7;

                case "CN":
                    return ProductEditionEnum.SmallBusiness;

                case "CX":
                    return ProductEditionEnum.EnterpriseMarketingSolution;

                default:
                    throw new InvalidOperationException("License key is not valid.");
            }
        }

        #endregion


        #region "Web farm"

        /// <summary>
        /// Runs the processing of specific web farm task for current provider
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Custom data</param>
        /// <param name="binary">Binary data</param>
        public override void ProcessWebFarmTask(string actionName, string data, byte[] binary)
        {
            // Switch by action name
            switch (actionName)
            {
                case "licensekeyinfoprovider":
                    {
                        RemoveFromStaticCollection(data);
                        C.Clear();
                        LicenseHelper.Clear();
                    }
                    break;

                // If action name is not handled throw an exception
                default:
                    throw new InvalidOperationException("The action name '" + actionName + "' has no supporting code.");
            }
        }

        #endregion
    }
}
