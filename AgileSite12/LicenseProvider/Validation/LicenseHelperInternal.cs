using System;
using System.Data;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.LicenseProvider
{
    /// <summary>
    /// Internal class for LicenseHelper. It contains all external dependencies.
    /// </summary>
    internal class LicenseHelperInternal
    {
        #region "Variables"

        // Hashtable to store result of checked URLs. [domain -> LicenseKeyInfo]
        private readonly CMSStatic<SafeDictionary<string, LicenseKeyInfo>> mA7BF6 = new CMSStatic<SafeDictionary<string, LicenseKeyInfo>>(() => new SafeDictionary<string, LicenseKeyInfo>(StringComparer.InvariantCultureIgnoreCase));

        // Site IDs by domain
        private readonly CMSStatic<SafeDictionary<string, int?>> mSiteIds = new CMSStatic<SafeDictionary<string, int?>>(() => new SafeDictionary<string, int?>());

        // License version blog table
        private readonly CMSStatic<SafeDictionary<string, int?>> mEA5 = new CMSStatic<SafeDictionary<string, int?>>(() => new SafeDictionary<string, int?>());

        // License version document table
        private readonly CMSStatic<SafeDictionary<string, int?>> mA7FB6 = new CMSStatic<SafeDictionary<string, int?>>(() => new SafeDictionary<string, int?>());

        // Logging policy with 15 minutes period
        private static Lazy<LoggingPolicy> licenseLoggingPolicy = new Lazy<LoggingPolicy>(() =>
        {
            return new LoggingPolicy(TimeSpan.FromMinutes(15));
        });

        #endregion


        #region "Properties"

        /// <summary>
        /// Specifies application expiration date.
        /// </summary>
        internal virtual DateTime A7FB5
        {
            get
            {
                return DateTime.MinValue;
            }
        }


        /// <summary>
        /// Hashtable to store result of checked URLs. [domain -> LicenseKeyInfo]
        /// </summary>
        internal SafeDictionary<string, LicenseKeyInfo> A7BF6
        {
            get { return mA7BF6; }
        }


        /// <summary>
        /// Site IDs by domain
        /// </summary>
        internal SafeDictionary<string, int?> SiteIds
        {
            get { return mSiteIds; }
        }


        /// <summary>
        /// License version blog table
        /// </summary>
        internal SafeDictionary<string, int?> EA5
        {
            get { return mEA5; }
        }


        /// <summary>
        /// License version document table
        /// </summary>
        internal SafeDictionary<string, int?> A7FB6
        {
            get { return mA7FB6; }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Validates license for specified domain.
        /// </summary>
        /// <param name="domain">Domain</param>
        internal virtual LicenseValidationEnum ValidateLicenseForDomain(string domain)
        {
            if (String.IsNullOrEmpty(domain))
            {
                return LicenseValidationEnum.Valid;
            }

            // Set default validation result to not available
            LicenseValidationEnum returnResult = LicenseValidationEnum.NotAvailable;

            // Convert domain to lower case
            domain = ParseDomainName(domain);

            // Get license key info form cache
            LicenseKeyInfo lki = A7BF6[domain];

            // Check whether license key info for current domain is cached
            if (lki != null)
            {
                // Get license validation result, cached
                returnResult = lki.ValidationResult;

                // Check whether cached license key info hasn't just expired
                if ((returnResult == LicenseValidationEnum.Valid) && (lki.ExpirationDateReal != LicenseKeyInfo.TIME_UNLIMITED_LICENSE) && (lki.ExpirationDateReal < DateTime.Now))
                {
                    // If is expired, remove key from cache and set expired status
                    LicenseKeyInfoProvider.RemoveFromStaticCollection(domain);
                    A7BF6.Remove(domain);

                    returnResult = LicenseValidationEnum.Expired;
                }
            }
            else
            {
                // Get license key for current domain
                lki = LicenseKeyInfoProvider.GetLicenseKeyInfo(domain);

                // If license key exists cache current license key info
                if (lki != null)
                {
                    A7BF6[domain] = lki;
                    returnResult = lki.ValidationResult;
                }
            }

            return returnResult;
        }


        /// <summary>
        /// Creates warning record in event log.
        /// </summary>        
        /// <param name="feature">Feature which will be used as event source</param>
        /// <param name="message">Message describing the license problem</param>
        internal virtual void LogLicenseEvent(FeatureEnum feature, string message)
        {
            EventLogProvider.LogEvent(EventType.WARNING, feature.ToString(), LicenseHelper.LICENSE_LIMITATION_EVENTCODE, message, loggingPolicy: licenseLoggingPolicy.Value);
        }


        /// <summary>
        /// Reports the invalid license, either by redirecting to given URL, or by throwing an exception .
        /// </summary>
        /// <param name="redirectUrl">URL to redirect.</param>
        /// <param name="message">Message to report</param>
        internal virtual void ReportLicenseError(string redirectUrl, string message = null)
        {
            if (!String.IsNullOrEmpty(redirectUrl) && CMSActionContext.CurrentAllowLicenseRedirect && (CMSHttpContext.Current != null))
            {
                // Redirect to license error page
                URLHelper.Redirect(redirectUrl);
            }
            else
            {
                throw new LicenseException(message);
            }
        }


        /// <summary>
        /// Returns whether is feature available for given domain.
        /// </summary>
        /// <param name="domain">Domain.</param>
        /// <param name="feature">Feature to check.</param>
        internal virtual bool IsFeatureAvailable(string domain, FeatureEnum feature)
        {
            return LicenseKeyInfoProvider.IsFeatureAvailable(domain, feature);
        }


        /// <summary>
        /// Returns "License limitation" resource string.
        /// </summary>
        internal virtual string GetLicenseLimitationString()
        {
            return CoreServices.Localization.GetString("licenselimitation.featurenotavailable");
        }


        /// <summary>
        /// Returns "Feature limit exceeded" resource string.
        /// </summary>
        internal virtual string GetFeatureLimitExceededString()
        {
            return CoreServices.Localization.GetString("licenselimitation.featurelimitexceeded");
        }


        /// <summary>
        /// Parses domain name from URL.
        /// </summary>
        /// <param name="url">URL to parse.</param>
        internal virtual string ParseDomainName(string url)
        {
            return LicenseKeyInfoProvider.ParseDomainName(url);
        }


        /// <summary>
        /// Calls query which returns dataset with SiteID and SiteStatus of all sites which uses given domain.
        /// </summary>
        /// <param name="domain">Domain.</param>
        internal virtual DataSet GetSiteIDFromSiteTable(string domain)
        {
            return ConnectionHelper.ExecuteQuery("SELECT SiteID, SiteStatus FROM CMS_Site WHERE ( SiteDomainName = '" + domain + "' OR SiteDomainName LIKE '" + domain + ":%' )", null, QueryTypeEnum.SQLQuery);
        }


        /// <summary>
        /// Calls query which returns SiteID and SiteStatus of all sites which uses given domain as an domain alias.
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        internal virtual DataSet GetSiteIDFromSiteAliasTable(string domain)
        {
            return ConnectionHelper.ExecuteQuery("SELECT SiteID, SiteStatus FROM CMS_Site WHERE SiteID IN ( SELECT SiteID FROM CMS_SiteDomainAlias WHERE SiteDomainAliasName='" + domain + "' OR SiteDomainAliasName LIKE '" + domain + ":%' )", null, QueryTypeEnum.SQLQuery);
        }


        /// <summary>
        /// Returns version limitations.
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="feature">Feature type</param>
        /// <param name="siteCheck">If true limitations are not applied under URLs in Site manager, CMS Desk, CMSModules and CMSPages/Logon</param>
        internal virtual int VersionLimitations(string domain, FeatureEnum feature, bool siteCheck = true)
        {
            return LicenseKeyInfoProvider.VersionLimitations(domain, feature, siteCheck);
        }


        /// <summary>
        /// Returns DataClassInfo object according to 
        /// </summary>
        /// <param name="className">Class name.</param>
        internal virtual DataClassInfo GetDataClassInfo(string className)
        {
            return DataClassInfoProvider.GetDataClassInfo(className);
        }


        /// <summary>
        /// Calls query which returns number of blog documents for given domain.
        /// </summary>
        /// <param name="classID">Class ID of blog docuemnt type.</param>
        /// <param name="domain">Domain.</param>
        internal virtual DataSet GetNumberOfBlogDocuments(int classID, string domain)
        {
            return ConnectionHelper.ExecuteQuery("SELECT COUNT(*) AS NUM FROM CMS_Tree WHERE NodeClassID = " + classID + " AND NodeSiteID = " + LicenseHelper.GetSiteIDbyDomain(domain), null, QueryTypeEnum.SQLQuery);
        }


        /// <summary>
        /// Calls query which returns number of documents for current domain.
        /// </summary>
        /// <param name="domain">Domain.</param>
        internal virtual DataSet GetNumberOfDocuments(string domain)
        {
            return ConnectionHelper.ExecuteQuery("SELECT COUNT(*) AS NUM FROM CMS_Tree WHERE NodeSiteID = " + LicenseHelper.GetSiteIDbyDomain(domain), null, QueryTypeEnum.SQLQuery);
        }


        /// <summary>
        /// Returns true if the feature is available for current URL.
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="feature">Feature type</param>
        internal virtual bool CheckFeature(string url, FeatureEnum feature)
        {
            string domain = LicenseKeyInfoProvider.ParseDomainName(url);

            return LicenseKeyInfoProvider.IsFeatureAvailable(domain, feature);
        }


        /// <summary>
        /// Indicates if the module is loaded.
        /// </summary>
        /// <param name="moduleName">Module name</param>
        internal virtual bool IsModuleLoaded(string moduleName)
        {
            return ModuleEntryManager.IsModuleLoaded(moduleName);
        }


        /// <summary>
        /// Determines whether the user interface, which is unavailable due to the license, should be hidden from all sites.
        /// </summary>       
        internal virtual bool IsUnavailableUIHidden()
        {
            return CoreServices.Settings["CMSHideUnavailableUserInterface"].ToBoolean(false);
        }

        #endregion
    }
}
