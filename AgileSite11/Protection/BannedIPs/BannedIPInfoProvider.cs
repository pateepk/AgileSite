using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;

namespace CMS.Protection
{
    /// <summary>
    /// Class providing BannedIPInfo management.
    /// </summary>
    public class BannedIPInfoProvider : AbstractInfoProvider<BannedIPInfo, BannedIPInfoProvider>
    {
        #region "Private fields"

        // Name of WF task that reloads the cached banned ips.
        private const string RELOAD_BANNED_IPS_WF_TASK = "ReloadCachedBannedIPs";

        // Constant used as part of global not overridable cache key (The square brackets are used because of possible collision with site code name)
        private const string GLOBAL_NOT_OVERRIDABLE = "_[global-not-override-able]_";

        // Lock object for concurrent initial load of cached banned IPs
        private static readonly object loadLock = new object();

        // Lock object for concurrent reload of cached banned IPs
        private static readonly object reloadLock = new object();

        // Stores global banned IPs and if it is allowed.
        private readonly CMSStatic<List<BannedIPInfo>> mGlobalBannedIPs = new CMSStatic<List<BannedIPInfo>>();

        // Stores site related banned IPs and if it is allowed.
        private readonly CMSStatic<List<BannedIPInfo>> mSitesBannedIPs = new CMSStatic<List<BannedIPInfo>>();

        // Contains denied IP address which accessed site from last change in banned IP info objects.
        private readonly CMSStatic<HashSet<string>> mCachedBannedIPs = new CMSStatic<HashSet<string>>(() => new HashSet<string>());

        // Sets the last change date time.
        private DateTime mLastChangeInternal = DateTime.Now;

        #endregion


        #region "Properties"

        /// <summary>
        /// Stores global banned IPs and if it is allowed.
        /// </summary>
        protected List<BannedIPInfo> GlobalBannedIPs
        {
            get
            {
                return mGlobalBannedIPs;
            }
            set
            {
                mGlobalBannedIPs.Value = value;
            }
        }


        /// <summary>
        /// Stores site related banned IPs and if it is allowed.
        /// </summary>
        protected List<BannedIPInfo> SitesBannedIPs
        {
            get
            {
                return mSitesBannedIPs;
            }
            set
            {
                mSitesBannedIPs.Value = value;
            }
        }


        /// <summary>
        /// Contains denied IP address which accessed site from last change in banned IP info objects.
        /// Increases performance when attacker is spamming.
        /// </summary>
        protected HashSet<string> CachedBannedIPs
        {
            get
            {
                return mCachedBannedIPs;
            }
            set
            {
                mCachedBannedIPs.Value = value;
            }
        }


        /// <summary>
        /// Returns time of the last change in banned IP settings.
        /// </summary>
        public static DateTime LastChange
        {
            get
            {
                return ProviderObject.LastChangeInternal;
            }
        }


        /// <summary>
        /// Returns time of the last change in banned IP settings.
        /// </summary>
        protected DateTime LastChangeInternal
        {
            get
            {
                return mLastChangeInternal;
            }
            set
            {
                mLastChangeInternal = value;
            }
        }

        #endregion


        #region "Public constants"

        /// <summary>
        /// Bool indicating that IP was allowed.
        /// </summary>
        public const bool ALLOWED = true;

        /// <summary>
        /// Bool indicating that IP was denied.
        /// </summary>
        public const bool DENIED = false;

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns the BannedIPInfo structure for the specified bannedIP.
        /// </summary>
        /// <param name="bannedIPId">BannedIP id</param>
        public static BannedIPInfo GetBannedIPInfo(int bannedIPId)
        {
            return ProviderObject.GetInfoById(bannedIPId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified bannedIP.
        /// </summary>
        /// <param name="bannedIP">BannedIP to set</param>
        public static void SetBannedIPInfo(BannedIPInfo bannedIP)
        {
            ProviderObject.SetInfo(bannedIP);
        }


        /// <summary>
        /// Deletes specified bannedIP.
        /// </summary>
        /// <param name="infoObj">BannedIP object</param>
        public static void DeleteBannedIPInfo(BannedIPInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified bannedIP.
        /// </summary>
        /// <param name="bannedIPId">BannedIP id</param>
        public static void DeleteBannedIPInfo(int bannedIPId)
        {
            BannedIPInfo infoObj = GetBannedIPInfo(bannedIPId);
            DeleteBannedIPInfo(infoObj);
        }


        /// <summary>
        /// Returns the query for all banned IPs.
        /// </summary>   
        public static ObjectQuery<BannedIPInfo> GetBannedIPs()
        {
            return ProviderObject.GetBannedIPsInternal();
        }


        /// <summary>
        /// Returns true if client IP address (from HttpContext) is allowed to access the site, false if it is banned.
        /// </summary>
        /// <param name="siteName">Ban settings of this site will be used</param>   
        /// <param name="banType">Which ban type should be checked</param>
        public static bool IsAllowed(string siteName, BanControlEnum banType)
        {
            if (CMSHttpContext.Current != null)
            {
                return IsAllowed(RequestContext.UserHostAddress, siteName, banType);
            }

            return ALLOWED;
        }


        /// <summary>
        /// Returns true if client IP address is allowed to access the site, false if it is banned.
        /// </summary>
        /// <param name="ipAddress">Check this ip address</param>
        /// <param name="siteName">Ban settings of this site will be used</param>        
        /// <param name="banType">Which ban type should be checked</param>
        public static bool IsAllowed(string ipAddress, string siteName, BanControlEnum banType)
        {
            return ProviderObject.IsAllowedInternal(ipAddress, siteName, banType);
        }


        /// <summary>
        /// Check if IP address id allowed and if not redirects to the page specified in App settings, key CMSBannedIPRedirectURL.
        /// </summary>
        /// <param name="siteName">Ban settings of this site will be used</param>
        /// <param name="banType">Which ban type should be checked</param>
        public static bool CheckIPandRedirect(string siteName, BanControlEnum banType)
        {
            if (IsAllowed(siteName, banType))
            {
                return ALLOWED;
            }

            BanRedirect(siteName);

            return DENIED;
        }


        /// <summary>
        /// Checks the current IP for being banned for web access
        /// </summary>
        public static void CheckBannedIP()
        {
            // Get sitename
            string siteName = SiteContext.CurrentSiteName;

            // Process banned IPs
            if (String.IsNullOrEmpty(siteName) || !IsBannedIPEnabled(siteName))
            {
                return;
            }
            
            // Check if this session was banned
            if (!IsAllowed(siteName, BanControlEnum.Complete))
            {
                BanRedirect(siteName);
            }
        }


        /// <summary>
        /// Redirects current http response to special page for banned IPs (settings key CMSBannedIPRedirectURL).
        /// </summary>
        public static void BanRedirect(string siteName)
        {
            if (HttpContext.Current == null)
            {
                return;
            }

            string cmsBannedIPRedirectURL = URLHelper.ResolveUrl(SettingsKeyInfoProvider.GetValue(siteName + ".CMSBannedIPRedirectURL"));
            string url = RequestContext.URL.GetLeftPart(UriPartial.Path);

            if (!url.EndsWith(cmsBannedIPRedirectURL, StringComparison.InvariantCultureIgnoreCase))
            {
                URLHelper.Redirect(cmsBannedIPRedirectURL);
            }
        }


        /// <summary>
        /// Returns true if BannedIP module is enabled for specified site (Settings key).
        /// </summary>
        public static bool IsBannedIPEnabled(string siteName)
        {
            return ProviderObject.IsBannedIPEnabledInternal(siteName);
        }

        #endregion


        #region "Cache methods"

        /// <summary>
        /// Load all banned ip from DB to Hash tables if not yet loaded.
        /// </summary>
        protected virtual void LoadCachedIPs()
        {
            if ((GlobalBannedIPs == null) || (SitesBannedIPs == null))
            {
                lock (loadLock)
                {
                    if ((GlobalBannedIPs == null) || (SitesBannedIPs == null))
                    {
                        ReloadCachedIPs(true);
                    }
                }
            }
        }


        /// <summary>
        /// Reload all banned ip from DB to Hash tables.
        /// </summary>
        /// <param name="logWebFarm">Enables or disables webfarm task logging, if false no task is logged</param>
        protected virtual void ReloadCachedIPs(bool logWebFarm)
        {
            // Get fresh values from DB
            var newGlobalIPs = new List<BannedIPInfo>();
            var newSiteIPs = new List<BannedIPInfo>();

            var reloadStarted = DateTime.Now;

            foreach (BannedIPInfo bi in GetBannedIPs())
            {
                if (bi.IPAddressSiteID == 0)
                {
                    newGlobalIPs.Add(bi);
                }
                else
                {
                    newSiteIPs.Add(bi);
                }
            }

            if (ProviderObject.LastChangeInternal <= reloadStarted)
            {
                lock (reloadLock)
                {
                    if (ProviderObject.LastChangeInternal <= reloadStarted)
                    {
                        // Swap old collections with new, so that currently running operations continue working with old snapshot
                        GlobalBannedIPs = newGlobalIPs;
                        SitesBannedIPs = newSiteIPs;

                        // Clear cached IPs, because of change in BannedIP info objects
                        CachedBannedIPs = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

                        ProviderObject.LastChangeInternal = reloadStarted;

                        // Create webfarm task if needed
                        if (logWebFarm)
                        {
                            ProviderObject.CreateWebFarmTask(RELOAD_BANNED_IPS_WF_TASK, String.Empty);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Runs the processing of specific web farm task for current provider
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Custom task data</param>
        /// <param name="binary">Binary data</param>
        public override void ProcessWebFarmTask(string actionName, string data, byte[] binary)
        {
            switch (actionName)
            {
                case RELOAD_BANNED_IPS_WF_TASK:
                    ReloadCachedIPs(false);
                    break;

                // If action name is not handled throw an exception
                default:
                    throw new Exception("[" + TypeInfo.ObjectType + ".ProcessWebFarmTask] The action name '" + actionName + "' has no supporting code.");
            }
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Gets the info by its ID.
        /// </summary>
        /// <param name="id">ID of the object</param>
        /// <param name="useHashtable">If true, the get operation uses hashtable</param>
        protected override BannedIPInfo GetInfoById(int id, bool useHashtable = true)
        {
            LoadCachedIPs();

            // Not using hashtables, get directly
            if (!useHashtable)
            {
                return GetObjectQuery().WhereEquals(TypeInfo.IDColumn, id).TopN(1).FirstOrDefault();
            }

            // Look for cached banned IPs before querying the DB
            return (GlobalBannedIPs.FirstOrDefault(ip => ip.IPAddressID == id))
                ?? (SitesBannedIPs.FirstOrDefault(ip => ip.IPAddressID == id))
                ?? GetObjectQuery().WhereEquals(TypeInfo.IDColumn, id).TopN(1).FirstOrDefault();
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(BannedIPInfo info)
        {
            if (info != null)
            {
                // Save to the database
                base.SetInfo(info);

                // Update hashtables
                ReloadCachedIPs(true);
            }
            else
            {
                throw new Exception("[BannedIPInfoProvider.SetBannedIPInfo]: No BannedIPInfo object set.");
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(BannedIPInfo info)
        {
            if (info != null)
            {
                // Save to the database
                base.DeleteInfo(info);

                // Update hashtables
                ReloadCachedIPs(true);
            }
        }


        /// <summary>
        /// Returns true if BannedIP module is enabled for specified site (Settings key).
        /// </summary>
        protected virtual bool IsBannedIPEnabledInternal(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSBannedIPEnabled");
        }


        /// <summary>
        /// Returns the query for all banned IPs.
        /// </summary>   
        protected virtual ObjectQuery<BannedIPInfo> GetBannedIPsInternal()
        {
            return GetObjectQuery();
        }


        /// <summary>
        /// Returns true if client IP address is allowed to access the site, false if it is banned.
        /// </summary>
        /// <param name="ipAddress">Check this IP address</param>
        /// <param name="siteName">Ban settings of this site will be used</param>        
        /// <param name="banType">Which ban type should be checked</param>
        protected virtual bool IsAllowedInternal(string ipAddress, string siteName, BanControlEnum banType)
        {
            // Default behavior is to allow
            if (String.IsNullOrEmpty(ipAddress) || !IsBannedIPEnabled(siteName) || !LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.BannedIP))
            {
                return ALLOWED;
            }

            LoadCachedIPs();

            string banTypeString = banType.ToStringRepresentation();
            string globalIpAddressMask = ipAddress + GLOBAL_NOT_OVERRIDABLE + banTypeString;
            string siteIpAddressMask = ipAddress + "_" + siteName + "_" + banTypeString;

            // IP status is in global not overridable cache
            if (CachedBannedIPs.Contains(globalIpAddressMask))
            {
                return DENIED;
            }

            // IP status for site is in cache
            if (!String.IsNullOrEmpty(siteName) && CachedBannedIPs.Contains(siteIpAddressMask))
            {
                return DENIED;
            }

            bool ipStatus = ALLOWED;
            bool allowOverride = true;

            int banTypeMask = (int)banType;

            // Login and Registration must be covered by All non complete option
            if ((banType == BanControlEnum.Registration) || (banType == BanControlEnum.Login))
            {
                banTypeMask = banTypeMask | (int)BanControlEnum.AllNonComplete;
            }

            BannedIPInfo appliedGlobalInfo = GetMatchingBannedIpInfo(GlobalBannedIPs, ipAddress, banTypeMask);
            if (appliedGlobalInfo != null)
            {
                if (appliedGlobalInfo.IPAddressAllowed)
                {
                    // If IP is allowed in global and site cannot override it, the result is final
                    if (!appliedGlobalInfo.IPAddressAllowOverride)
                    {
                        return ALLOWED;
                    }
                }
                else
                {
                    ipStatus = DENIED;
                    allowOverride = appliedGlobalInfo.IPAddressAllowOverride;

                    // Cache result.
                    if (!allowOverride)
                    {
                        CachedBannedIPs.Add(globalIpAddressMask);
                    }
                }
            }

            // Do it only if site can override global settings
            if (allowOverride && !String.IsNullOrEmpty(siteName))
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
                if (si != null)
                {
                    var appliedSiteInfo = GetMatchingBannedIpInfo(SitesBannedIPs.Where(bi => (bi != null) && (bi.IPAddressSiteID == si.SiteID)), ipAddress, banTypeMask);
                    if ((appliedSiteInfo != null) && ((appliedGlobalInfo == null) || CompareIpAddressSpecificity(appliedSiteInfo, appliedGlobalInfo) >= 0))
                    {
                        if (appliedSiteInfo.IPAddressAllowed)
                        {
                            return ALLOWED;
                        }

                        CachedBannedIPs.Add(siteIpAddressMask);
                        return DENIED;
                    }
                }
            }

            return ipStatus;
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Returns regular expression corresponding to ip address.
        /// </summary>
        /// <param name="ipAddress">Ip address with special marks</param>        
        public static string GetRegularIPAddress(string ipAddress)
        {
            // Special treatment for *
            string addressRegular = "";
            if (ipAddress != null)
            {
                addressRegular = ipAddress.Replace(".", "\\.").Replace("*", "[0-9a-fA-F]*") + "$";
            }

            return addressRegular;
        }

        
        /// <summary>
        /// Gets BannedIPInfo from the given list that best match the given address a ban type mask.
        /// </summary>
        private BannedIPInfo GetMatchingBannedIpInfo(IEnumerable<BannedIPInfo> bannedIpInfos, string ipAddress, int banTypeMask)
        {
            BannedIPInfo matchignInfo = null;
            Func<BannedIPInfo, BannedIPInfo, int> bannedIpComparer = (a, b) =>
            {
                var res = CompareIpAddressSpecificity(a, b);
                if (res == 0)
                {
                    // Deny has higher priority than allow
                    return a.IPAddressAllowed ? -1 : 1;
                }

                return res;
            };
            foreach (var banInfo in bannedIpInfos)
            {
                var banType = banInfo.IPAddressBanType.ToEnum<BanControlEnum>();

                // Check only for enabled banned IPs with right ban type
                if ((banInfo != null) && banInfo.IPAddressBanEnabled && ((banType == BanControlEnum.Complete) || (((int)banType & banTypeMask) > 0)))
                {
                    if (Regex.IsMatch(ipAddress, banInfo.IPAddressRegular) && ((matchignInfo == null) || bannedIpComparer(banInfo, matchignInfo) > 0))
                    {
                        matchignInfo = banInfo;
                    }
                }
            }

            return matchignInfo;
        }


        /// <summary>
        /// Returns positive value if the first info is more specific than the second. Negative value means the second info is more specific. Zero means equal specificity.
        /// </summary>
        private int CompareIpAddressSpecificity(BannedIPInfo a, BannedIPInfo b)
        {
            var bannedIpWildcardCount = a.IPAddress.Count(x => x == '*');
            var anotherBannedIpWildcardCount = b.IPAddress.Count(x => x == '*');

            if (bannedIpWildcardCount == anotherBannedIpWildcardCount)
            {
                if (a.IPAddressAllowed == b.IPAddressAllowed)
                {
                    return 0;
                }
            }
            if (bannedIpWildcardCount > anotherBannedIpWildcardCount)
            {
                return -1;
            }

            return 1;
        }

        #endregion
    }
}