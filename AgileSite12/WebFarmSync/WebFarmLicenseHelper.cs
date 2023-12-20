using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.CollectionExtensions;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Contains methods related to licensing of web farms. 
    /// </summary>
    internal class WebFarmLicenseHelper
    {
        #region "Constants"

        private const string LICENSE_CHECK_KEY = "WebFarmLicense";

        private const string LICENSE_CHECK_PUBLIC_DOMAIN_KEY = "WebFarmLicensePublicDomain";
        
        private const string LICENSE_SERVER_DOMAINS_KEY = "WebFarmServerDomains";

        private const string LICENSE_SERVER_SITES_KEY = "WebFarmServerSites";

        private const string LICENSE_SERVER_SITES_COUNT_KEY = "WebFarmServerSitesCount";

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if for given <paramref name="domainName"/> the <see cref="FeatureEnum.Webfarm"/> feature is enabled and not exceeded.
        /// </summary>
        /// <param name="domainName">Domain to check.</param>
        /// <remarks>
        /// For request to public domain without a license, check only if number of healthy webfarm servers is smaller 
        /// or equals to the webfarm count of the best possible license.
        /// For request to public domain with a license that supports webfarms, check if number of healthy webfarm servers is smaller 
        /// or equals to the webfarm count of the best possible license.
        /// For request to public domain with a license that does not support webfarms, false is returned.
        /// If the method is called without <paramref name="domainName"/>, e.g. from worker thread, or for local domain, only check if the number of healthy webfarm servers is smaller 
        /// or equals to the webfarm count of the best possible license. 
        /// </remarks>
        internal static bool IsWebFarmLicenseValid(string domainName = null)
        {
            var usedDomainName = domainName ?? RequestContext.CurrentDomain;
            if (String.IsNullOrEmpty(usedDomainName) || LicenseKeyInfoProvider.IsLocalDomain(usedDomainName))
            {
                // a. License is not available because the method is probably executed in worker thread (without domain context)
                // b. Domain is local domain
                return IsLicenseValid(allowLocalDomainLicenses: true);
            }

            // If we have public domain license available, check if the feature is available in the given license
            return (LicenseKeyInfoProvider.IsFeatureAvailable(usedDomainName, FeatureEnum.Webfarm) || LicenseKeyInfoProvider.GetLicenseKeyInfo(usedDomainName) == null) && IsLicenseValid(allowLocalDomainLicenses: false);
        }


        /// <summary>
        /// Checks the license policy for web farms.
        /// Returns true if current license is valid and does not exceed the license limitations.
        /// </summary>
        /// <param name="allowLocalDomainLicenses">If true, licenses for local domains are also taken into consideration when checking the maximum allowed number of web farm servers. Otherwise only public domain licenses are checked.</param>
        private static bool IsLicenseValid(bool allowLocalDomainLicenses)
        {
            var cacheKey = allowLocalDomainLicenses ? LICENSE_CHECK_KEY : LICENSE_CHECK_PUBLIC_DOMAIN_KEY;
            return ProtectedCacheHelper.Cache(cs => IsLicenseValidInternal(cs, allowLocalDomainLicenses), 5.0, cacheKey);
        }


        /// <summary>
        /// Checks the license policy for web farms.
        /// Returns true if current license is valid and does not exceed the license limitations.
        /// </summary>
        /// <param name="cacheSettings">Cache settings.</param>
        /// <param name="allowLocalDomainLicenses">If true, licenses for local domains are also taken into consideration when checking the maximum allowed number of web farm servers. Otherwise only public domain licenses are checked.</param>
        private static bool IsLicenseValidInternal(CacheSettings cacheSettings, bool allowLocalDomainLicenses)
        {
            var isLicenseValid = IsCurrentWebFarmServerCountAllowed(allowLocalDomainLicenses);

            if (cacheSettings.Cached)
            {
                cacheSettings.CacheDependency = CacheHelper.GetCacheDependency(new[]
                {
                    $"{WebFarmServerLogInfo.OBJECT_TYPE}|all",
                    $"{WebFarmServerInfo.OBJECT_TYPE}|all",
                    $"{LicenseKeyInfo.OBJECT_TYPE}|all",
                    $"{SiteInfo.OBJECT_TYPE}|all"
                });
            }
            
            return isLicenseValid;
        }


        /// <summary>
        /// Checks if current number of enabled web farm servers does not exceed the license limitations.
        /// </summary>
        /// <param name="allowLocalDomainLicenses">If true, licenses for local domains are also taken into consideration when checking the maximum allowed number of web farm servers. Otherwise only public domain licenses are checked.</param>
        private static bool IsCurrentWebFarmServerCountAllowed(bool allowLocalDomainLicenses)
        {
            // Application is not initialized when ApplicationInitialized is null or false
            if (!CMSActionContext.CurrentCheckLicense || CMSApplication.ApplicationInitialized != true || WebFarmContext.WebFarmMode == WebFarmModeEnum.Disabled)
            {
                return true;
            }

            var currentlyUsedServers = GetServerCountForLicenseCheck();

            IEnumerable<LicenseKeyInfo> licenseKeys = LicenseKeyInfoProvider.GetLicenseKeys()
                                                                            .ToList();

            if (!allowLocalDomainLicenses)
            {
                licenseKeys = licenseKeys.Where(lki => !LicenseKeyInfoProvider.IsLocalDomain(lki.Domain));
            }

            // Return true if there is any license that is valid for the number of web farm servers
            return licenseKeys.Any(l => LicenseKeyInfoProvider.IsFeatureAvailable(l, FeatureEnum.Webfarm) &&
                                                   (l.ValidationResult == LicenseValidationEnum.Valid) &&
                                                   ((l.LicenseServers == 0) || (l.LicenseServers >= currentlyUsedServers)));
        }

      
        /// <summary>
        /// Checks license for feature <see cref="FeatureEnum.Webfarm"/> for given <paramref name="domainName"/>.
        /// </summary>
        /// <param name="domainName">Domain to check.</param>
        /// <exception cref="LicenseException">Throws <see cref="LicenseException"/> if license check failed.</exception>
        internal static void CheckLicense(string domainName)
        {
            if (!IsWebFarmLicenseValid(domainName))
            {
                LicenseHelper.ReportFailedLicenseCheck(FeatureEnum.Webfarm, domainName, true);
            }
        }


        /// <summary>
        /// Gets number of servers that should count towards license check.
        /// This means servers that are responding regularly.
        /// </summary>
        internal static int GetServerCountForLicenseCheck()
        {
            var activeWebFarmServers = GetActiveWebFarmServers();
            var freeExternalWebAppServersCount = GetFreeExternalWebAppServersCount(activeWebFarmServers);

            return activeWebFarmServers.Count - freeExternalWebAppServersCount;
        }


        /// <summary>
        /// Gets enabled healthy web farm servers.
        /// </summary>
        private static List<WebFarmServerInfo> GetActiveWebFarmServers()
        {
            return WebFarmServerMonitoringInfoProvider.MonitoringData
                                                      .Where(s => s.Key.ServerEnabled && (s.Key.Status == WebFarmServerStatusEnum.Healthy))
                                                      .Select(s => s.Key)
                                                      .ToList();
        }


        /// <summary>
        /// Gets count of web farm servers, which are not counted in licensed servers.
        /// </summary>
        /// <param name="activeWebFarmServers">Active web farm servers.</param>
        private static int GetFreeExternalWebAppServersCount(List<WebFarmServerInfo> activeWebFarmServers)
        {
            var externalWebAppServersCount = activeWebFarmServers.Count(s => s.IsExternalWebAppServer);

            if (externalWebAppServersCount <= 0)
            {
                return 0;
            }

            var licensedDomains = ProtectedCacheHelper.Cache(GetLicensedDomains, 60.0, LICENSE_SERVER_DOMAINS_KEY);
            var runningSites = ProtectedCacheHelper.Cache(GetRunningSites, 60.0, LICENSE_SERVER_SITES_KEY);
            var licensedRunningSitesCount = ProtectedCacheHelper.Cache(cs => GetLicensedRunningSitesCount(cs, licensedDomains, runningSites), 60.0, LICENSE_SERVER_SITES_COUNT_KEY);

            return Math.Min(externalWebAppServersCount, licensedRunningSitesCount);
        }


        /// <summary>
        /// Gets all domains, which have valid license.
        /// </summary>
        /// <param name="cacheSettings">Cache settings.</param>
        private static HashSet<string> GetLicensedDomains(CacheSettings cacheSettings)
        {
            var licensedDomains = LicenseKeyInfoProvider.GetLicenseKeys()
                                                        .ToList()
                                                        .Where(lki => lki.ValidationResult == LicenseValidationEnum.Valid)
                                                        .Select(lki => lki.Domain)
                                                        .ToHashSetCollection(StringComparer.InvariantCultureIgnoreCase);

            if (cacheSettings.Cached)
            {
                cacheSettings.CacheDependency = CacheHelper.GetCacheDependency($"{LicenseKeyInfo.OBJECT_TYPE}|all");
            }

            return licensedDomains;
        }


        /// <summary>
        /// Gets all sites, which are currently running.
        /// </summary>
        /// <param name="cacheSettings">Cache settings.</param>
        private static List<SiteInfo> GetRunningSites(CacheSettings cacheSettings)
        {
            var runningSites = SiteInfoProvider.GetSites().Where("SiteStatus", QueryOperator.Equals, SiteStatusEnum.Running.ToStringRepresentation()).ToList();

            if (cacheSettings.Cached)
            {
                cacheSettings.CacheDependency = CacheHelper.GetCacheDependency($"{SiteInfo.OBJECT_TYPE}|all");
            }

            return runningSites;
        }


        /// <summary>
        /// Gets count of sites, which have at least one domain with valid license.
        /// </summary>
        /// <param name="cacheSettings">Cache settings.</param>
        /// <param name="licensedDomains">Licensesd domains.</param>
        /// <param name="runningSites">Running sites.</param>
        private static int GetLicensedRunningSitesCount(CacheSettings cacheSettings, HashSet<string> licensedDomains, List<SiteInfo> runningSites)
        {
            var licensedRunningSitesCount = runningSites
                .Count(si => licensedDomains.Contains(si.DomainName) || licensedDomains.Any(ld => si.SiteDomainAliases.ContainsKey(ld.ToLowerInvariant())));

            if (cacheSettings.Cached)
            {
                cacheSettings.CacheDependency = CacheHelper.GetCacheDependency(new[] {
                    $"{LicenseKeyInfo.OBJECT_TYPE}|all",
                    $"{SiteInfo.OBJECT_TYPE}|all"
                });
            }

            return licensedRunningSitesCount;
        }

        #endregion
    }
}
