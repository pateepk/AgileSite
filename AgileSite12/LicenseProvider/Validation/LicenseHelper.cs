using System;
using System.Data;
using System.Text;
using System.Web;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.LicenseProvider
{
    /// <summary>
    /// LicenseHelper class.
    /// </summary>
    public static class LicenseHelper
    {
        #region "Constants"

        /// <summary>
        /// Event log messages about version limitations will have this event code.
        /// </summary>
        public const string LICENSE_LIMITATION_EVENTCODE = "LICENSELIMITATION";

        /// <summary>
        /// Number of items(given feature) is not limited in product version.
        /// </summary>
        public const int LIMITATIONS_UNLIMITED = 0;

        /// <summary>
        /// No items(given feature) is allowed in product version.
        /// </summary>
        public const int LIMITATIONS_NOITEMS = -1;

        /// <summary>
        /// Allows basic workflow(no custom steps) in product version.
        /// </summary>
        public const int LIMITATIONS_BASICWORKFLOW = 1;

        #endregion


        #region "Variables"

        private static LicenseHelperInternal mInternalHelper;

        #endregion


        #region "Properties"

        /// <summary>
        /// The time when the application expires.
        /// </summary>
        public static DateTime ApplicationExpires
        {
            get
            {
                return InternalHelper.A7FB5;
            }
        }


        /// <summary>
        /// Gets product edition for current domain.
        /// </summary>
        public static ProductEditionEnum CurrentEdition
        {
            get
            {
                var lki = LicenseContext.CurrentLicenseInfo;
                if (lki != null)
                {
                    return lki.Edition;
                }

                return ProductEditionEnum.Free;
            }
        }


        /// <summary>
        /// Gets or sets internal helper instance.
        /// </summary>
        internal static LicenseHelperInternal InternalHelper
        {
            get
            {
                return mInternalHelper ?? (mInternalHelper = new LicenseHelperInternal());
            }
            set
            {
                mInternalHelper = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Checks that number of <paramref name="feature"/>-related objects on current website does not exceed the license limitations.
        /// </summary>
        /// <param name="feature">Feature to be checked</param>
        /// <param name="currentObjectCount">Number of feature-related objects on current website</param>
        /// <param name="maxObjectCount">Maximum allowed number of feature-related objects. 0 is unlimited</param>
        public static bool CheckLicenseLimitations(FeatureEnum feature, out int currentObjectCount, out int maxObjectCount)
        {
            var eventArgs = new ObjectCountCheckEventArgs
            {
                Feature = feature
            };

            LicenseCheckEvents.ObjectCountCheckEvent.StartEvent(eventArgs); 
             
            currentObjectCount = eventArgs.ObjectCount;
            maxObjectCount = LicenseKeyInfoProvider.VersionLimitations(RequestContext.CurrentDomain, feature, false);

            return maxObjectCount == LIMITATIONS_UNLIMITED || currentObjectCount <= maxObjectCount;
        }


        /// <summary>
        /// Returns true if the version is valid.
        /// </summary>
        internal static bool CheckApplicationValidity()
        {
            if ((InternalHelper.A7FB5 != DateTime.MinValue) && (InternalHelper.A7FB5 < DateTime.Now))
            {
                throw new InvalidOperationException(Encoding.ASCII.GetString(Convert.FromBase64String("VGhpcyBhcHBsaWNhdGlvbiBpcyBubyBsb25nZXIgdmFsaWQu")));
            }

            return true;
        }


        /// <summary>
        /// Validates license for specified domain.
        /// </summary>
        /// <param name="domain">Domain</param>
        public static LicenseValidationEnum ValidateLicenseForDomain(string domain)
        {
            return InternalHelper.ValidateLicenseForDomain(domain);
        }


        /// <summary>
        /// Reports that the license limit was exceeded
        /// </summary>
        /// <param name="feature">Feature enum</param>
        public static void GetAllAvailableKeys(FeatureEnum feature)
        {
            // Log event
            InternalHelper.LogLicenseEvent(feature, feature.ToString());

            bool check = true;

            // Check if it is not request for license limitation page - to prevent cycling(Windows authentication)
            if (CMSHttpContext.Current != null)
            {
                string url = CMSHttpContext.Current.Request.AppRelativeCurrentExecutionFilePath;

                check = !url.StartsWith("~/CMSMessages/LicenseLimit", StringComparison.InvariantCultureIgnoreCase);
            }

            if (check)
            {
                // Report license error
                var redirectUrl = HttpUtility.UrlPathEncode(Encoding.ASCII.GetString(Convert.FromBase64String("fi9DTVNNZXNzYWdlcy9MaWNlbnNlTGltaXQuYXNweD9mZWF0dXJlPQ==")) + feature);
                var message = String.Format(Encoding.ASCII.GetString(Convert.FromBase64String("VGhlIGxpY2VuY2UgbGltaXQgZm9yIGZlYXR1cmUgezB9IHdhcyBleGNlZWRlZA==")), feature);

                InternalHelper.ReportLicenseError(redirectUrl, message);
            }
        }


        /// <summary>
        /// Gets the license key for the URL and checks if the feature is supported for its product edition. 
        /// If not, it redirects the user to ~/CMSMessages/FeatureNotAvailable.aspx
        /// </summary>
        /// <param name="domain">Domain</param>
        /// <param name="feature">Feature type</param>
        public static bool CheckFeatureAndRedirect(string domain, FeatureEnum feature)
        {
            if (CMSHttpContext.Current != null)
            {
                if (!InternalHelper.IsFeatureAvailable(domain, feature))
                {
                    // Log license limitation and redirect
                    InternalHelper.LogLicenseEvent(feature, String.Format(InternalHelper.GetLicenseLimitationString(), feature.ToString()));

                    var redirectUrl = HttpUtility.UrlPathEncode(Encoding.ASCII.GetString(Convert.FromBase64String("fi9DTVNNZXNzYWdlcy9GZWF0dXJlTm90QXZhaWxhYmxlLmFzcHg/ZG9tYWlubmFtZT0=")) + HttpUtility.UrlEncode(domain));
                    var message = String.Format(Encoding.ASCII.GetString(Convert.FromBase64String("RmVhdHVyZSB7MH0gaXMgbm90IGF2YWlsYWJsZSBpbiBjdXJyZW50IGxpY2Vuc2Uu")), feature);

                    InternalHelper.ReportLicenseError(redirectUrl, message);

                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Returns true if the feature is available for current URL.
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="feature">Feature type</param>
        public static bool CheckFeature(string url, FeatureEnum feature)
        {
            return InternalHelper.CheckFeature(url, feature);
        }


        /// <summary>
        /// Gets the license key for the URL and checks if the feature is supported for its product edition. 
        /// If not, it throws and exception.
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="feature">Feature type</param>
        public static void RequestFeature(string url, FeatureEnum feature)
        {
            url = url ?? RequestContext.CurrentDomain;

            if (String.IsNullOrEmpty(url))
            {
                return;
            }

            string domain = LicenseKeyInfoProvider.ParseDomainName(url);

            if (!InternalHelper.IsFeatureAvailable(domain, feature))
            {
                throw new LicenseException(Encoding.ASCII.GetString(Convert.FromBase64String("VGhlIGZlYXR1cmUgJw==")) + feature + Encoding.ASCII.GetString(Convert.FromBase64String("JyBpcyBub3Qgc3VwcG9ydGVkIGluIHRoaXMgZWRpdGlvbi4=")));
            }
        }


        /// <summary>
        /// Checks if the WebDAV feature is supported for current domain. 
        /// If not, it throws and exception.
        /// </summary>
        public static void RequestWebDAVFeature()
        {
            RequestFeature(RequestContext.CurrentDomain, FeatureEnum.WebDav);
        }


        /// <summary>
        /// Clear license limitations tables.
        /// </summary>
        public static void ClearLicenseLimitation(bool logTasks = true)
        {
            InternalHelper.EA5.Clear();
            InternalHelper.A7FB6.Clear();
            InternalHelper.SiteIds.Clear();

            if (logTasks)
            {
                WebFarmHelper.CreateTask(new ClearLicenseLimitationCacheWebFarmTask());
            }
        }


        /// <summary>
        /// Returns site id by domains.
        /// </summary>
        /// <param name="domain">Domain name</param>
        public static int GetSiteIDbyDomain(string domain)
        {
            if (domain == null)
            {
                return 0;
            }

            var domainToLower = domain.ToLowerInvariant();

            if (InternalHelper.SiteIds.TryGetValue(domainToLower, out var siteId))
            {
                return ValidationHelper.GetInteger(siteId, 0);
            }

            domain = SqlHelper.GetSafeQueryString(domain, false);

            // Get site list for selected domain
            DataSet ds = InternalHelper.GetSiteIDFromSiteTable(domain);
            int result = GetSiteIDFromDataSet(ds);
            if (result == 0)
            {
                ds = InternalHelper.GetSiteIDFromSiteAliasTable(domain);
                result = GetSiteIDFromDataSet(ds);
            }

            if (result != 0)
            {
                InternalHelper.SiteIds[domainToLower] = result;
                return result;
            }

            return 0;
        }


        /// <summary>
        /// License version checker.
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="feature">Feature type</param>
        /// <param name="action">Type of action - edit, insert, delete</param>
        /// <returns>Returns true if feature is without any limitations for domain and action</returns>
        public static bool LicenseVersionCheck(string domain, FeatureEnum feature, ObjectActionEnum action)
        {
            // Parse domain name to remove port etc.
            if (domain != null)
            {
                domain = LicenseKeyInfoProvider.ParseDomainName(domain);
            }

            // Check version limitation
            int versionLimitations = InternalHelper.VersionLimitations(domain, feature, (action != ObjectActionEnum.Insert));

            if (versionLimitations == 0)
            {
                return true;
            }

            // Log security license limitation
            var sdr = SecurityDebug.LogSecurityOperation(domain, "LicenseLimitation", null, feature.ToString(), false, null, SecurityDebug.Settings.CurrentIndent++);

            // Blogs
            if (feature == FeatureEnum.Blogs)
            {
                if (InternalHelper.EA5[domain] == null)
                {
                    DataClassInfo dci = InternalHelper.GetDataClassInfo("cms.blog");

                    if (dci != null)
                    {
                        DataSet forumSite = InternalHelper.GetNumberOfBlogDocuments(dci.ClassID, domain);
                        if (!DataHelper.DataSourceIsEmpty(forumSite))
                        {
                            InternalHelper.EA5[domain] = DataHelper.GetIntValue(forumSite.Tables[0].Rows[0], "Num");
                        }
                    }
                }

                try
                {
                    // Try add
                    if (action == ObjectActionEnum.Insert)
                    {
                        if (versionLimitations < ValidationHelper.GetInteger(InternalHelper.EA5[domain], -1) + 1)
                        {
                            return false;
                        }
                    }

                    // Get status
                    if (action == ObjectActionEnum.Edit)
                    {
                        if (versionLimitations < ValidationHelper.GetInteger(InternalHelper.EA5[domain], 0))
                        {
                            return false;
                        }
                    }
                }
                catch
                {
                    ClearLicenseLimitation();
                    return false;
                }
            }

            // Documents
            if (feature == FeatureEnum.Documents)
            {
                if (InternalHelper.A7FB6[domain] == null)
                {
                    DataSet forumSite = InternalHelper.GetNumberOfDocuments(domain);
                    if (!DataHelper.DataSourceIsEmpty(forumSite))
                    {
                        InternalHelper.A7FB6[domain] = DataHelper.GetIntValue(forumSite.Tables[0].Rows[0], "Num");
                    }
                }

                try
                {
                    // Try add
                    if (action == ObjectActionEnum.Insert)
                    {
                        if (versionLimitations < ValidationHelper.GetInteger(InternalHelper.A7FB6[domain], -1) + 1)
                        {
                            return false;
                        }
                    }

                    // Get status
                    if (action == ObjectActionEnum.Edit)
                    {
                        if (versionLimitations < ValidationHelper.GetInteger(InternalHelper.A7FB6[domain], 0))
                        {
                            return false;
                        }
                    }
                }
                catch
                {
                    ClearLicenseLimitation();
                    return false;
                }
            }

            // Log result
            if (sdr != null)
            {
                SecurityDebug.FinishSecurityOperation(sdr, null, null, null, true, null);
            }

            return true;
        }


        /// <summary>
        /// Returns edition name.
        /// </summary>
        /// <param name="edition">Edition type</param>        
        public static string GetEditionName(ProductEditionEnum edition)
        {
            switch (edition)
            {
                case ProductEditionEnum.Free:
                    return CoreServices.Localization.GetAPIString("Edition.F", null, "Free");
                    
                case ProductEditionEnum.UltimateV7:
                    return CoreServices.Localization.GetAPIString("Edition.V", null, "Ultimate");

                case ProductEditionEnum.Base:
                    return CoreServices.Localization.GetAPIString("Edition.B", null, "Base");

                case ProductEditionEnum.SmallBusiness:
                    return CoreServices.Localization.GetAPIString("Edition.N", null, "Small business");

                case ProductEditionEnum.EnterpriseMarketingSolution:
                    return CoreServices.Localization.GetAPIString("Edition.X", null, "Enterprise marketing solution");
            }

            return "";
        }
        

        /// <summary>
        /// Indicates if specified feature is available based on whether equivalent module is loaded and specified feature is available in user's best license.
        /// </summary>
        /// <param name="feature">Feature to check if it is available in user's best license.</param>         
        /// <param name="moduleName">Module to check if it is loaded. The check is omitted if module name is null or empty.</param>
        public static bool IsFeatureAvailableInBestLicense(FeatureEnum feature, string moduleName = null)
        {
            var bestLicense = LicenseKeyInfoProvider.GetBestLicense();
            var featureAvailable = LicenseKeyInfoProvider.IsFeatureAvailable(bestLicense, feature);

            return (IsModuleLoadedOrNull(moduleName) && featureAvailable);
        }


        /// <summary>
        /// Indicates if specified feature is available in UI based on whether equivalent module is loaded and specified feature is available in user's license.
        /// </summary>
        /// <param name="feature">Feature to check if it is available in user's license.</param>         
        /// <param name="moduleName">Module to check if it is loaded. The check is omitted if module name is null or empty.</param>        
        public static bool IsFeatureAvailableInUI(FeatureEnum feature, string moduleName = null)
        {
            return IsModuleLoadedOrNull(moduleName) && (!IsUnavailableUIHidden() || CheckFeature(RequestContext.CurrentDomain, feature));
        }


        /// <summary>
        /// Checks if module is loaded.
        /// </summary>
        /// <param name="moduleName">Module to check if it is loaded.</param>
        /// <returns>True if module is loaded or <paramref name="moduleName"/> is <c>null</c></returns>
        private static bool IsModuleLoadedOrNull(string moduleName)
        {
            return String.IsNullOrEmpty(moduleName) || InternalHelper.IsModuleLoaded(moduleName);
        }


        /// <summary>
        /// Determines whether the user interface, which is unavailable due to the license, should be hidden from all sites.
        /// </summary>  
        public static bool IsUnavailableUIHidden()
        {
            return InternalHelper.IsUnavailableUIHidden();
        }


        /// <summary>
        /// Clears the hashtable with URL results.
        /// </summary>
        public static void Clear()
        {
            lock (InternalHelper.A7BF6)
            {
                InternalHelper.A7BF6.Clear();
            }
        }


        /// <summary>
        /// Converts license validation result to its string representation.
        /// </summary>
        /// <param name="result">License validation result.</param>
        public static string GetValidationResultString(LicenseValidationEnum result)
        {
            string stringResult;

            switch (result)
            {
                case LicenseValidationEnum.Expired:
                    stringResult = "KeyExpired";
                    break;

                case LicenseValidationEnum.Invalid:
                    stringResult = "InvalidKey";
                    break;

                case LicenseValidationEnum.Valid:
                    stringResult = "ValidKey";
                    break;

                case LicenseValidationEnum.WrongFormat:
                    stringResult = "WrongFormat";
                    break;

                case LicenseValidationEnum.Unknown:
                    stringResult = "Unknown";
                    break;

                default:
                    stringResult = "NotAvailable";
                    break;
            }

            return CoreServices.Localization.GetString("invalidlicense." + stringResult);
        }


        /// <summary>
        /// Creates trial license keys.
        /// </summary>
        /// <param name="trialKey">Trial license key</param>
        /// <param name="deleteKeysFirst">If true all previous keys is deleted</param>
        /// <param name="ignoreExpired">If true expired licenses is ignored</param>
        public static bool AddTrialLicenseKeys(string trialKey, bool deleteKeysFirst, bool ignoreExpired)
        {
            bool result = false;
            if (trialKey != String.Empty)
            {
                if (deleteKeysFirst)
                {
                    // Delete all existing license keys
                    var licenses = LicenseKeyInfoProvider.GetLicenseKeys();
                    foreach (var license in licenses) 
                    {
                        LicenseKeyInfoProvider.DeleteLicenseKeyInfo(license);
                    }
                }

                // License key for 'localhost' and '127.0.0.1' domain
                LicenseKeyInfo licenseInfo = new LicenseKeyInfo();
                licenseInfo.LoadLicense(trialKey, "localhost");

                if (ignoreExpired || (licenseInfo.ValidationResult != LicenseValidationEnum.Expired))
                {
                    LicenseKeyInfoProvider.SetLicenseKeyInfo(licenseInfo);

                    licenseInfo = new LicenseKeyInfo();
                    licenseInfo.LoadLicense(trialKey, "127.0.0.1");
                    LicenseKeyInfoProvider.SetLicenseKeyInfo(licenseInfo);
                    result = true;
                }
            }
            return result;
        }


        /// <summary>
        /// Reports that <paramref name="feature"/> limit is exceeded for <paramref name="domain"/> license.
        /// </summary>
        /// <param name="feature">Feature for which the limit is exceeded.</param>
        /// <param name="domain">Domain with insufficient license.</param>
        public static void ReportExceededFeatureLimit(FeatureEnum feature, string domain)
        {
            InternalHelper.LogLicenseEvent(feature, String.Format(InternalHelper.GetFeatureLimitExceededString(), feature));
        }


        /// <summary>
        /// Reports failed license check for given <paramref name="feature"/>.
        /// </summary>
        /// <param name="feature">Feature for which license check failed.</param>
        /// <param name="domain">Domain with insufficient license.</param>
        /// <param name="throwError">Indicates whether <see cref="LicenseException"/> should be thrown or just failed attempt should be logged.</param>
        public static void ReportFailedLicenseCheck(FeatureEnum feature, string domain, bool throwError)
        {
            InternalHelper.LogLicenseEvent(feature, String.Format(InternalHelper.GetLicenseLimitationString(), feature));

            if (throwError)
            {
                // Report the error
                var redirectUrl = HttpUtility.UrlPathEncode(String.Format(
                    Encoding.ASCII.GetString(Convert.FromBase64String("fi9jbXNtZXNzYWdlcy9GZWF0dXJlTm90QXZhaWxhYmxlLmFzcHg/ZG9tYWlubmFtZT17MH0mZmVhdHVyZT17MX0=")),
                    HttpUtility.UrlEncode(domain),
                    HttpUtility.UrlEncode(feature.ToStringRepresentation())
                ));

                var message = $"License for feature '{feature}' not found.";

                InternalHelper.ReportLicenseError(redirectUrl, message);
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns Site ID from given dataset when SiteStatus for given site in dataset is running.
        /// </summary>
        /// <param name="ds">DataSet.</param>
        private static int GetSiteIDFromDataSet(DataSet ds)
        {
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Find SiteID for current domain
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    if (ValidationHelper.GetString(dr["SiteStatus"], "stopped").ToLowerInvariant() == "running")
                    {
                        return ValidationHelper.GetInteger(dr["SiteID"], 0);
                    }
                }
            }

            return 0;
        }

        #endregion
    }
}
