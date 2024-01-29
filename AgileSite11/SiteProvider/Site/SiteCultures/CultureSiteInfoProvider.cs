using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Localization;

namespace CMS.SiteProvider
{
    using TypedDataSet = InfoDataSet<CultureSiteInfo>;

    using CulturesTable = SafeDictionary<string, InfoDataSet<CultureInfo>>;
    using CultureCodesTable = SafeDictionary<string, List<string>>;
    using CultureCountTable = SafeDictionary<string, int?>;

    /// <summary>
    /// Class providing CultureSiteInfo management.
    /// </summary>
    public class CultureSiteInfoProvider : AbstractInfoProvider<CultureSiteInfo, CultureSiteInfoProvider>
    {
        #region "Private fields"

        /// <summary>
        /// Cultures per site - DataSet indexed by siteName. [siteName -> TypedDataSet]
        /// </summary>
        private static CMSStatic<CulturesTable> mSiteCultures = new CMSStatic<CulturesTable>(() => new CulturesTable());

        /// <summary>
        /// Culture codes per site - ArrayList indexed by siteName. [siteName -> List[string]]
        /// </summary>
        private static CMSStatic<CultureCodesTable> mSiteCultureCodes = new CMSStatic<CultureCodesTable>(() => new CultureCodesTable());

        /// <summary>
        /// Cultures count per site - int indexed by domain name. [domainName -> int]
        /// </summary>
        private static CMSStatic<CultureCountTable> mSiteCultureCount = new CMSStatic<CultureCountTable>(() => new CultureCountTable());


        /// <summary>
        /// Table lock for loading.
        /// </summary>
        private static object tableLock = new object();

        #endregion


        #region "Properties"

        /// <summary>
        /// Cultures per site - DataSet indexed by siteName. [siteName -> TypedDataSet]
        /// </summary>
        private static CulturesTable SiteCultures
        {
            get
            {
                return mSiteCultures;
            }
            set
            {
                mSiteCultures.Value = value;
            }
        }


        /// <summary>
        /// Culture codes per site - ArrayList indexed by siteName. [siteName -> List[string]]
        /// </summary>
        private static CultureCodesTable SiteCultureCodes
        {
            get
            {
                return mSiteCultureCodes;
            }
            set
            {
                mSiteCultureCodes.Value = value;
            }
        }


        /// <summary>
        /// Cultures count per site - int indexed by domain name. [domainName -> int]
        /// </summary>
        private static CultureCountTable SiteCultureCount
        {
            get
            {
                return mSiteCultureCount;
            }
            set
            {
                mSiteCultureCount.Value = value;
            }
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns all bindings between cultures and sites.
        /// </summary>
        public static ObjectQuery<CultureSiteInfo> GetCultureSites()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the bindings between cultures and sites.
        /// </summary>
        /// <param name="columns">Data columns to return</param>
        /// <param name="where">Where condition to filter data</param>
        /// <param name="orderBy">Order by statement</param>
        /// <param name="topN">Specifies number of returned records</param>        
        public static TypedDataSet GetCultureSites(string columns, string where, string orderBy, int topN)
        {
            return GetCultureSites()
                    .Columns(SqlHelper.ParseColumnList(columns))
                    .Where(where)
                    .OrderBy(orderBy)
                    .TopN(topN)
                    .TypedResult;
        }


        /// <summary>
        /// Returns the CultureSiteInfo structure for the specified cultureSite.
        /// </summary>
        /// <param name="cultureId">CultureID</param>
        /// <param name="siteId">SiteID</param>
        public static CultureSiteInfo GetCultureSiteInfo(int cultureId, int siteId)
        {
            return ProviderObject.GetCultureSiteInfoInternal(cultureId, siteId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified cultureSite.
        /// </summary>
        /// <param name="cultureSite">CultureSite to set</param>
        public static void SetCultureSiteInfo(CultureSiteInfo cultureSite)
        {
            ProviderObject.SetCultureSiteInfoInternal(cultureSite);
        }


        /// <summary>
        /// Deletes specified cultureSite.
        /// </summary>
        /// <param name="infoObj">CultureSite object</param>
        public static void DeleteCultureSiteInfo(CultureSiteInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified cultureSite.
        /// </summary>
        /// <param name="cultureId">CultureID</param>
        /// <param name="siteId">SiteID</param>
        public static void RemoveCultureFromSite(int cultureId, int siteId)
        {
            CultureSiteInfo infoObj = GetCultureSiteInfo(cultureId, siteId);
            DeleteCultureSiteInfo(infoObj);
        }


        /// <summary>
        /// Adds the class to the specified site.
        /// </summary>
        /// <param name="cultureId">CultureID</param>
        /// <param name="siteId">SiteID</param>
        /// <exception cref="Exception">Throws Exception if license limitations are not fulfilled.</exception>
        public static void AddCultureToSite(int cultureId, int siteId)
        {
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteId);
            if (si != null)
            {
                if (!LicenseVersionCheckInternal(si.DomainName, FeatureEnum.Multilingual, ObjectActionEnum.Insert, si.SiteID))
                {
                    throw new LicenseException(ResHelper.GetString("licenselimitation.siteculturesexceeded"));
                }

                // Create new binding
                CultureSiteInfo infoObj = new CultureSiteInfo();
                infoObj.CultureID = cultureId;
                infoObj.SiteID = siteId;

                // Save to the database
                SetCultureSiteInfo(infoObj);
            }
        }


        /// <summary>
        /// Determines whether specified culture is on specified site.
        /// </summary>
        /// <param name="cultureId">Culture ID</param>
        /// <param name="siteId">Site ID</param>
        public static bool IsCultureOnSite(int cultureId, int siteId)
        {
            var culture = CultureInfoProvider.GetCultureInfo(cultureId);
            var site = SiteInfoProvider.GetSiteInfo(siteId);
            if ((culture != null) && (site != null))
            {
                return IsCultureOnSite(culture.CultureCode, site.SiteName);
            }

            return false;
        }


        /// <summary>
        /// Returns the dataset containing the sites of the specified culture.
        /// </summary>
        /// <param name="cultureCode">Culture code</param>
        public static InfoDataSet<SiteInfo> GetCultureSites(string cultureCode)
        {
            return ProviderObject.GetCultureSitesInternal(cultureCode);
        }


        /// <summary>
        /// Returns the dataset containing the sites of the specified culture.
        /// </summary>
        /// <param name="cultureId">Culture id</param>
        /// <param name="orderBy">ORDER BX expression</param>
        [Obsolete("Use CMS.DataEngine.ObjectQuery instead.")]
        public static InfoDataSet<SiteInfo> GetCultureSites(int cultureId, string orderBy)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@CultureID", cultureId);
            parameters.EnsureDataSet<SiteInfo>();

            // Get the data
            return ConnectionHelper.ExecuteQuery("cms.culture.selectsites", parameters, null, orderBy).As<SiteInfo>();
        }


        /// <summary>
        /// Adds culture to specified site.
        /// </summary>
        /// <param name="culturedCode">Code of culture to add</param>
        /// <param name="siteName">Name of the site</param>
        public static void AddCultureToSite(string culturedCode, string siteName)
        {
            // Get the objects
            CultureInfo ci = CultureInfoProvider.GetCultureInfo(culturedCode);
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if ((ci != null) && (si != null))
            {
                AddCultureToSite(ci.CultureID, si.SiteID);
            }
        }


        /// <summary>
        /// Determines whether specified culture is on specified site.
        /// </summary>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="siteName">Site name</param>
        public static bool IsCultureOnSite(string cultureCode, string siteName)
        {
            if (string.IsNullOrEmpty(cultureCode) || string.IsNullOrEmpty(siteName))
            {
                return false;
            }

            // Get site cultures
            var cultures = GetSiteCultureCodes(siteName);
            if (cultures == null)
            {
                return false;
            }

            // Check given culture if in site cultures
            return cultures.Exists(c => c.Equals(cultureCode, StringComparison.OrdinalIgnoreCase));
        }


        /// <summary>
        /// Remove culture from the specified site.
        /// </summary>
        /// <param name="cultureCode">Code of culture to remove from the site</param>
        /// <param name="siteName">Name of the site</param>
        public static void RemoveCultureFromSite(string cultureCode, string siteName)
        {
            CultureInfo ci = CultureInfoProvider.GetCultureInfo(cultureCode);
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if ((ci != null) && (si != null))
            {
                RemoveCultureFromSite(ci.CultureID, si.SiteID);
            }
        }


        /// <summary>
        /// Returns a list containing the culture codes of the specified site.
        /// If this feature not available for license type, returns only the first culture found
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static List<string> GetSiteCultureCodes(string siteName)
        {
            siteName = siteName.ToLowerInvariant();
            var result = SiteCultureCodes[siteName];
            if (result == null)
            {
                // Build the list of site cultures
                var cultures = GetSiteCultures(siteName);
                if (cultures != null)
                {
                    result = cultures.Select(c => c.CultureCode).ToList();
                }

                // Cache to the hashtable
                SiteCultureCodes[siteName] = result;
            }

            return result;
        }


        /// <summary>
        /// Returns the dataset containing the cultures of the specified site.
        /// If this feature not available for license type, returns only the first culture in the dataset
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static InfoDataSet<CultureInfo> GetSiteCultures(string siteName)
        {
            string lowerSiteName = siteName.ToLowerInvariant();

            // Try to get from hashtable
            var cultures = SiteCultures[lowerSiteName];
            if (cultures == null)
            {
                lock (tableLock)
                {
                    // Check if not loaded already
                    cultures = SiteCultures[lowerSiteName];
                    if (cultures == null)
                    {
                        cultures = ProviderObject.LoadSiteCulturesInternal(SiteInfoProvider.GetSiteID(siteName));
                        SiteCultures[lowerSiteName] = cultures;
                    }
                }
            }

            return cultures;
        }


        /// <summary>
        /// Removes all cultures from specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static void RemoveSiteCultures(string siteName)
        {
            // Get the cultures
            var cultures = GetSiteCultures(siteName);
            if (cultures != null)
            {
                // Remove all cultures
                foreach (var culture in cultures)
                {
                    RemoveCultureFromSite(culture.CultureCode, siteName);
                }
            }
        }


        /// <summary>
        /// Checks if the culture is allowed on the site. Returns true if the culture is allowed.
        /// </summary>
        /// <param name="culture">Culture to check</param>
        /// <param name="siteName">Site name</param>
        public static bool IsCultureAllowed(string culture, string siteName)
        {
            // If no site name is defined, all cultures allowed
            if (string.IsNullOrEmpty(siteName))
            {
                return true;
            }

            // Empty culture is never allowed
            if (string.IsNullOrEmpty(culture))
            {
                return false;
            }

            // Get the cultures
            var cultures = GetSiteCultures(siteName);

            return (cultures != null) && cultures.Any(c => culture.Equals(c.CultureCode, StringComparison.OrdinalIgnoreCase));
        }


        ///<summary>
        /// Finds out whether given site is multilingual
        ///</summary>
        ///<param name="siteName">Name of site</param>
        ///<returns>True if site is multilingual.</returns>
        public static bool IsSiteMultilingual(string siteName)
        {
            int currentCount = 0;

            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si != null)
            {
                string domain = si.DomainName.ToLowerInvariant();

                // Try to get from cache
                if (SiteCultureCount[domain] != null)
                {
                    currentCount = (int)SiteCultureCount[domain];
                }
                else
                {
                    // Get the cultures from the DB
                    var cultures = GetSiteCultures(siteName);
                    if (cultures != null)
                    {
                        SiteCultureCount[domain] = cultures.Count();
                    }
                }
            }

            return (currentCount > 1);
        }


        /// <summary>
        /// Check if culture code is in web, and return culture code, if no, return default culture code.
        /// </summary>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="siteName">Site name</param>
        public static string CheckCultureCode(string cultureCode, string siteName)
        {
            // Check if culture is available on this web
            if (IsCultureAllowed(cultureCode, siteName))
            {
                return cultureCode;
            }

            return CultureHelper.GetDefaultCultureCode(siteName);
        }


        /// <summary>
        /// Clears the site cultures hashtables.
        /// </summary>
        /// <param name="logWebFarmTask">Indicates whether webfarm task should be created</param>
        public static void ClearSiteCultures(bool logWebFarmTask)
        {
            lock (tableLock)
            {
                SiteCultures = new CulturesTable();
                SiteCultureCodes = new CultureCodesTable();
                SiteCultureCount = new CultureCountTable();

                // Create webfarm task if is required
                if (logWebFarmTask)
                {
                    ProviderObject.CreateWebFarmTask("clearsitecultures", String.Empty);
                }
            }
        }


        /// <summary>
        /// Checks if current domain has license supporting Multilingual feature.
        /// </summary>
        /// <returns>True if license includes the feature, otherwise false</returns>
        public static bool LicenseVersionCheck()
        {
            return LicenseVersionCheck(RequestContext.CurrentDomain, FeatureEnum.Multilingual, ObjectActionEnum.Edit);
        }


        /// <summary>
        /// Checks if domain has license supporting specified feature.
        /// </summary>
        /// <param name="domain">The domain carrying the license</param>
        /// <param name="feature">The feature to be checked</param>
        /// <param name="action">The action to complete. If it is insert action currentStatus count is increased to check the state after insertion</param>                
        /// <returns>True if license includes the feature, otherwise false</returns>
        public static bool LicenseVersionCheck(string domain, FeatureEnum feature, ObjectActionEnum action)
        {
            return LicenseVersionCheckInternal(domain, feature, action);
        }


        /// <summary>
        /// Checks if domain has license supporting specified feature.
        /// </summary>
        /// <param name="domain">The domain carrying the license</param>
        /// <param name="feature">The feature to be checked</param>
        /// <param name="action">The action to complete. If it is insert action currentStatus count is increased to check the state after insertion</param>                
        /// <param name="siteId">The site id of the site. If specified the license check is not using cache tables per domain</param>
        /// <returns>True if license includes the feature, otherwise false</returns>
        internal static bool LicenseVersionCheckInternal(string domain, FeatureEnum feature, ObjectActionEnum action, int siteId = 0)
        {
            if (String.IsNullOrEmpty(domain))
            {
                return true;
            }

            // Get current count of site cultures = languages
            int currentCount = 0;

            // Get culture count for specific site (incl. stopped site)
            if (siteId > 0)
            {
                var cultures = GetSiteCultures(SiteInfoProvider.GetSiteName(siteId));
                if (cultures != null)
                {
                    currentCount = cultures.Count();
                }
            }
            // Get culture count for site defined by domain
            else
            {
                // Try to get from cache
                domain = domain.ToLowerInvariant();
                if (SiteCultureCount[domain] != null)
                {
                    currentCount = (int)SiteCultureCount[domain];
                }
                else
                {
                    siteId = LicenseHelper.GetSiteIDbyDomain(domain);

                    // Get the cultures from the DB
                    var cultures = GetSiteCultures(SiteInfoProvider.GetSiteName(siteId));
                    if (cultures != null)
                    {
                        currentCount = cultures.Count();

                        // Store count to cache
                        SiteCultureCount[domain] = currentCount;
                    }
                }
            }

            // Increase count to check the state after insertion
            if (action == ObjectActionEnum.Insert)
            {
                currentCount++;
            }

            // Get limitations
            int versionLimitations = LicenseKeyInfoProvider.VersionLimitations(domain, feature, false);

            // Return true if feature is without any limitations
            if (versionLimitations == LicenseHelper.LIMITATIONS_UNLIMITED)
            {
                return true;
            }

            bool isOk = (currentCount <= versionLimitations);

            // Log license problem
            if (!isOk)
            {
                string message = String.Format(ResHelper.GetString("licenselimitation.siteculturesexceededlog"), domain);
                EventLogProvider.LogEvent(EventType.WARNING, "Cultures", LicenseHelper.LICENSE_LIMITATION_EVENTCODE, message, RequestContext.CurrentURL);
            }

            return isOk;
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the CultureSiteInfo structure for the specified cultureSite.
        /// </summary>
        /// <param name="cultureId">CultureID</param>
        /// <param name="siteId">SiteID</param>
        protected virtual CultureSiteInfo GetCultureSiteInfoInternal(int cultureId, int siteId)
        {
            return GetCultureSites().WhereEquals("CultureID", cultureId).OnSite(siteId, siteId == ProviderHelper.ALL_SITES).BinaryData(true).FirstObject;
        }


        /// <summary>
        /// Sets (updates or inserts) specified cultureSite.
        /// </summary>
        /// <param name="cultureSite">CultureSite to set</param>
        protected virtual void SetCultureSiteInfoInternal(CultureSiteInfo cultureSite)
        {
            if (cultureSite != null)
            {
                // Check IDs
                if ((cultureSite.SiteID <= 0) || (cultureSite.CultureID <= 0))
                {
                    throw new Exception("[CultureSiteInfoProvider.SetCultureSiteInfo]: Object IDs not set.");
                }

                // Get existing
                CultureSiteInfo existing = GetCultureSiteInfoInternal(cultureSite.CultureID, cultureSite.SiteID);
                if (existing != null)
                {
                    // Do nothing, item does not carry any data                    
                }
                else
                {
                    cultureSite.Generalized.InsertData();

                    // Remove cached cultures for site
                    ClearSiteCultures(true);
                }
            }
            else
            {
                throw new Exception("[CultureSiteInfoProvider.SetCultureSiteInfo]: No CultureSiteInfo object set.");
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(CultureSiteInfo info)
        {
            if (info != null)
            {
                // Delete the object
                base.DeleteInfo(info);

                // Remove cached cultures for site
                ClearSiteCultures(true);
            }
        }


        /// <summary>
        /// Returns the dataset containing the sites of the specified culture.
        /// </summary>
        /// <param name="cultureCode">Culture code</param>
        protected virtual InfoDataSet<SiteInfo> GetCultureSitesInternal(string cultureCode)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@CultureCode", cultureCode);
            parameters.EnsureDataSet<SiteInfo>();

            // Get the data
            return ConnectionHelper.ExecuteQuery("cms.site.selectsitesperculture", parameters).As<SiteInfo>();
        }


        /// <summary>
        /// Returns dataset with all cultures for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual InfoDataSet<CultureInfo> LoadSiteCulturesInternal(int siteId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteId);
            parameters.EnsureDataSet<CultureInfo>();

            // Get the cultures
            return ConnectionHelper.ExecuteQuery("cms.culture.selectsitecultures", parameters).As<CultureInfo>();
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
                // Clear site-culture hashtable
                case "clearsitecultures":
                    ClearSiteCultures(false);
                    break;

                // If action name is not handled throw an exception
                default:
                    throw new Exception("[" + TypeInfo.ObjectType + ".ProcessWebFarmTask] The action name '" + actionName + "' has no supporting code.");
            }
        }

        #endregion
    }
}