using System;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.LicenseProvider;

namespace CMS.SiteProvider
{
    /// <summary>
    /// Class to provide the Site management.
    /// </summary>
    public class SiteInfoProvider : AbstractInfoProvider<SiteInfo, SiteInfoProvider>
    {
        private const string CLEAR_SITE_ACTION_NAME = "clearsiteinfoprovider";


        #region "Private fields"

        // Table of the running site infos indexed by the domain names. [domainName -> SiteInfo]
        private static readonly CMSStatic<SafeDictionary<string, SiteInfo>> mRunningSites = new CMSStatic<SafeDictionary<string, SiteInfo>>(() => new SafeDictionary<string, SiteInfo>());

        // Empty site info
        private static SiteInfo mEmptySite;

        #endregion


        #region "Properties

        /// <summary>
        /// Empty site info
        /// </summary>
        private static SiteInfo EmptySite
        {
            get
            {
                return mEmptySite ?? (mEmptySite = new SiteInfo());
            }
        }


        /// <summary>
        /// Table of the running site infos indexed by the domain names. [domainName -> SiteInfo]
        /// </summary>
        private static SafeDictionary<string, SiteInfo> RunningSites
        {
            get
            {
                return mRunningSites;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public SiteInfoProvider()
            : base(SiteInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true,
                Load = LoadHashtableEnum.All
            })
        {
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns domain for specified culture
        /// </summary>
        /// <param name="siteName">Current site name</param>
        /// <param name="docCulture">Required culture</param>
        public static string GetDomainForCulture(string siteName, string docCulture)
        {
            return ProviderObject.GetDomainForCultureInternal(siteName, docCulture);
        }


        /// <summary>
        /// Ensures the site name value, if the value is null, sets the current site name to the value.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string EnsureSiteName(ref string siteName)
        {
            return siteName ?? (siteName = SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Returns the site info object with specified ID.
        /// </summary>
        /// <param name="siteId">Site ID for that retrieve the info structure</param>
        public static SiteInfo GetSiteInfo(int siteId)
        {
            return ProviderObject.GetInfoById(siteId);
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static SiteInfo GetSiteInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns the SiteInfo structure for a specified site.
        /// </summary>
        /// <param name="siteName">Site name for that retrieve the data</param>
        public static SiteInfo GetSiteInfo(string siteName)
        {
            return ProviderObject.GetInfoByCodeName(siteName);
        }


        /// <summary>
        /// Returns ID of the specified site, or 0 if site not found.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static int GetSiteID(string siteName)
        {
            return ProviderObject.GetSiteIDInternal(siteName);
        }


        /// <summary>
        /// Returns name of the specified site, or empty string if site not found.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static string GetSiteName(int siteId)
        {
            return ProviderObject.GetSiteNameInternal(siteId);
        }


        /// <summary>
        /// Sets the specified site data.
        /// </summary>
        /// <param name="siteInfo">New site info data</param>
        public static void SetSiteInfo(SiteInfo siteInfo)
        {
            ProviderObject.SetInfo(siteInfo);
        }


        /// <summary>
        /// Deletes the specified site.
        /// </summary>
        /// <param name="siteInfo">Site object to delete</param>
        public static void DeleteSiteInfo(SiteInfo siteInfo)
        {
            DeleteSiteInfo(new SiteDeletionSettings
            {
                Site = siteInfo
            });
        }


        /// <summary>
        /// Deletes the specified site.
        /// </summary>
        /// <param name="siteName">Site name to delete</param>
        public static void DeleteSiteInfo(string siteName)
        {
            SiteInfo si = GetSiteInfo(siteName);
            DeleteSiteInfo(si);
        }


        /// <summary>
        /// Deletes the site with all dependent objects. If <paramref name="progressLog"/> provided, all actions are logged to provide progress
        /// </summary>
        /// <param name="siteDeletionSettings">Site deletion settings</param>
        /// <param name="progressLog">Progress log</param>
        public static void DeleteSiteInfo(SiteDeletionSettings siteDeletionSettings, IProgress<SiteDeletionStatusMessage> progressLog = null)
        {
            LicenseCheckDisabler.ExecuteWithoutLicenseCheck(() => ProviderObject.DeleteSiteInfoInternal(siteDeletionSettings, progressLog));
        }


        /// <summary>
        /// Returns all site records.
        /// </summary>
        public static ObjectQuery<SiteInfo> GetSites()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets the specified Site running.
        /// </summary>
        /// <param name="siteName">Site name of the site to run</param>
        public static void RunSite(string siteName)
        {
            ProviderObject.RunSiteInternal(siteName);
        }


        /// <summary>
        /// Returns dataSet with document type classes of specified site.
        /// </summary>
        /// <param name="siteId">ID of site</param>
        public static InfoDataSet<DataClassInfo> GetDocumentTypeClassPerSite(int siteId)
        {
            return ProviderObject.GetDocumentTypeClassPerSiteInternal(siteId);
        }


        /// <summary>
        /// Returns WHERE condition for retrieving data according to the unique column and site ID column.
        /// </summary>
        /// <param name="uniqueColumn">Unique (code name or GUID) column name</param>
        /// <param name="uniqueColumnValue">Unique column value</param>
        /// <param name="siteIDColumn">Site ID column name</param>
        /// <param name="siteName">Site name</param>
        public static string GetSiteWhereCondition(string uniqueColumn, string uniqueColumnValue, string siteIDColumn, string siteName)
        {
            return ProviderObject.GetSiteWhereConditionInternal(uniqueColumn, uniqueColumnValue, siteIDColumn, siteName);
        }


        /// <summary>
        /// Combine with default culture?
        /// </summary>
        public static bool CombineWithDefaultCulture(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSCombineWithDefaultCulture");
        }


        /// <summary>
        /// Combine files with default culture?
        /// </summary>
        public static bool CombineFilesWithDefaultCulture(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSCombineImagesWithDefaultCulture");
        }


        /// <summary>
        /// Gets number of existing sites.
        /// </summary>
        public static int GetSitesCount()
        {
            return GetSites().GetCount();
        }


        /// <summary>
        /// Gets number of running sites.
        /// </summary>
        public static int GetRunningSitesCount()
        {
            return GetSites().WhereEquals("SiteStatus", "RUNNING").GetCount();
        }


        /// <summary>
        /// Returns the running SiteInfo selected by specified domain name, if the site is not running, returns null.
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="applicationPath">Application path</param>
        public static SiteInfo GetRunningSiteInfo(string domainName, string applicationPath)
        {
            if (String.IsNullOrEmpty(domainName))
            {
                return null;
            }
            
            SiteInfo result = GetRunningSiteInfoObject(domainName, applicationPath);

            // Try to find domain without "www."
            if (result != null)
            {
                return result;
            }

            bool hasWww = domainName.StartsWith("www.", StringComparison.InvariantCultureIgnoreCase);
            if (hasWww)
            {
                result = GetRunningSiteInfoObject(domainName.Remove(0, 4), applicationPath);
            }
            
            // Try to find domain without port number
            if (result != null)
            {
                return result;
            }

            var portIndex = domainName.IndexOf(':');
            if (portIndex <= 0)
            {
                return null;
            }

            domainName = domainName.Remove(portIndex);
            result = GetRunningSiteInfoObject(domainName, applicationPath);

            // Try to find domain without "www." and without port number
            if (result != null)
            {
                return result;
            }

            if (hasWww)
            {
                result = GetRunningSiteInfoObject(domainName.Remove(0, 4), applicationPath);
            }

            return result;
        }


        /// <summary>
        /// Sets the specified Site stopped.
        /// </summary>
        /// <param name="siteName">Site name of the site to stop</param>
        public static void StopSite(string siteName)
        {
            SiteInfo si = GetSiteInfo(siteName);
            if (si == null)
            {
                return;
            }

            // Save the status
            si.Status = SiteStatusEnum.Stopped;
            SetSiteInfo(si);

            // Clear cached cultures
            CultureSiteInfoProvider.ClearSiteCultures(true);
        }


        /// <summary>
        /// Returns site name from url.
        /// </summary>
        /// <param name="url">Url</param>
        public static string GetSiteNameFromUrl(string url)
        {
            return ProviderObject.GetSiteNameFromUrlInternal(url);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns running SiteInfo object from hashtable, if not found try to find it in database.
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="applicationPath">Application path</param>
        private static SiteInfo GetRunningSiteInfoObject(string domainName, string applicationPath)
        {
            SiteInfo result = null;

            if (!String.IsNullOrEmpty(domainName))
            {
                // If application path specified, get primarily by the application path
                if (!string.IsNullOrEmpty(applicationPath) && (applicationPath != "/"))
                {
                    string fullName = domainName + applicationPath;
                    result = GetRunningSiteInfoObject(fullName);
                }

                // Get the base alias by standard domain name if not found
                if (result == null)
                {
                    result = GetRunningSiteInfoObject(domainName);
                }
            }

            return result;
        }


        /// <summary>
        /// Returns running SiteInfo object from hashtable, if not found try to find it in database.
        /// </summary>
        /// <param name="domainName">Site alias (domain Name)</param>
        private static SiteInfo GetRunningSiteInfoObject(string domainName)
        {
            SiteInfo result = null;

            if (!String.IsNullOrEmpty(domainName))
            {
                domainName = domainName.ToLowerCSafe();
                SiteInfo storedResult = RunningSites[domainName];

                if (storedResult == null)
                {
                    // If not found in hashtable, try to find it in DB
                    result = GetRunningSiteInfoFromDB(domainName);

                    if (result == null)
                    {
                        RunningSites[domainName] = EmptySite;
                    }
                    else
                    {
                        bool domainFound = true;

                        // Check whether the running site was found due to PresentationURL
                        if (!String.IsNullOrEmpty(result.SitePresentationURL)
                            && !result.DomainName.Equals(domainName, StringComparison.InvariantCultureIgnoreCase)
                            && (result.SiteDomainAliases[domainName] == null))
                        {
                            // PresentationURL doesn't have to always match the searched domain due to SQL query: PresentationURL LIKE '%'+domainName+'%'
                            // => Process only exact domain name match
                            if (!String.Equals(result.SitePresentationDomain, domainName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                domainFound = false;
                                result = null;
                                RunningSites[domainName] = EmptySite;
                            }
                        }

                        if (domainFound)
                        {
                            RunningSites[domainName] = result;

                            // Cache the supplement domain (Presentation or Main domain)
                            if (!String.IsNullOrEmpty(result.SitePresentationDomain))
                            {
                                string supplementDomain = domainName.Equals(result.SitePresentationDomain, StringComparison.InvariantCultureIgnoreCase) ? result.DomainName : result.SitePresentationDomain;
                                RunningSites[supplementDomain.ToLowerCSafe()] = result;
                            }

                            // Get domain aliases
                            DataSet aliasDS = SiteDomainAliasInfoProvider.GetDomainAliases()
                                .WhereEquals("SiteID", result.SiteID)
                                .Column("SiteDomainAliasName");

                            if (!DataHelper.DataSourceIsEmpty(aliasDS))
                            {
                                foreach (DataRow dr in aliasDS.Tables[0].Rows)
                                {
                                    string alias = ValidationHelper.GetString(dr["SiteDomainAliasName"], "");
                                    if (alias != "")
                                    {
                                        RunningSites[alias.ToLowerCSafe()] = result;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (storedResult != EmptySite)
                {
                    // Use the result from hashtable
                    result = storedResult;
                }
            }

            return result;
        }


        /// <summary>
        /// Gets the running site info object from the database.
        /// </summary>
        /// <param name="domainName">Domain name</param>
        private static SiteInfo GetRunningSiteInfoFromDB(string domainName)
        {
            var siteIds = SiteDomainAliasInfoProvider.GetDomainAliases()
                .Column("SiteID")
                .WhereEquals("SiteDomainAliasName", domainName);

            var sites = GetSites()
                .Where(new WhereCondition()
                    .WhereEquals("SiteDomainName", domainName)
                    .Or()
                    .WhereContains("SitePresentationURL", domainName)
                    .Or()
                    .WhereIn("SiteID", siteIds))
                .WhereEquals("SiteStatus", "RUNNING")
                .OrderBy("SitePresentationURL");

            return sites.FirstOrDefault();
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns domain for specified culture
        /// </summary>
        /// <param name="siteName">Current site name</param>
        /// <param name="docCulture">Required culture</param>
        protected virtual string GetDomainForCultureInternal(string siteName, string docCulture)
        {
            string redirectDomain = String.Empty;

            SiteInfo si = GetSiteInfo(siteName);
            if (si == null)
            {
                return redirectDomain;
            }

            if (!String.IsNullOrEmpty(si.DefaultVisitorCulture) && docCulture.Equals(si.DefaultVisitorCulture, StringComparison.InvariantCultureIgnoreCase))
            {
                redirectDomain = si.DomainName;
            }
            else
            {
                // Check aliases
                DataSet ds = SiteDomainAliasInfoProvider.GetDomainAliases(si.SiteID);
                if (DataHelper.DataSourceIsEmpty(ds))
                {
                    return redirectDomain;
                }

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    // If domain alias uses specific culture, set the culture
                    string aliasCulture = ValidationHelper.GetString(dr["SiteDefaultVisitorCulture"], string.Empty);
                    if (!String.IsNullOrEmpty(aliasCulture) && docCulture.Equals(aliasCulture, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Check the domain name
                        redirectDomain = ValidationHelper.GetString(dr["SiteDomainAliasName"], string.Empty);
                    }
                }
            }

            return redirectDomain;
        }


        /// <summary>
        /// Returns ID of the specified site, or 0 if site not found.
        /// </summary>
        /// <param name="siteName">Site name</param>
        protected virtual int GetSiteIDInternal(string siteName)
        {
            // Get site ID
            SiteInfo si = GetSiteInfo(siteName);
            if (si != null)
            {
                return si.SiteID;
            }

            return 0;
        }


        /// <summary>
        /// Returns name of the specified site, or empty string if site not found.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual string GetSiteNameInternal(int siteId)
        {
            // Get site ID
            SiteInfo si = GetSiteInfo(siteId);
            if (si != null)
            {
                return si.SiteName;
            }

            return "";
        }


        /// <summary>
        /// Returns WHERE condition for retrieving data according to the unique column and site ID column.
        /// </summary>
        /// <param name="uniqueColumn">Unique (code name or GUID) column name</param>
        /// <param name="uniqueColumnValue">Unique column value</param>
        /// <param name="siteIDColumn">Site ID column name</param>
        /// <param name="siteName">Site name</param>
        protected virtual string GetSiteWhereConditionInternal(string uniqueColumn, string uniqueColumnValue, string siteIDColumn, string siteName)
        {
            string siteWhere = "";

            // Get Site WHERE condition
            if (!string.IsNullOrEmpty(siteIDColumn))
            {
                if (!string.IsNullOrEmpty(siteName))
                {
                    // Ensure string value
                    siteName = SqlHelper.EscapeQuotes(siteName);

                    siteWhere = string.Format("{0} IN (SELECT SiteID FROM CMS_Site WHERE SiteName = '{1}')", siteIDColumn, siteName);
                }
                else
                {
                    siteWhere = string.Format("{0} IS NULL)", siteIDColumn);
                }
            }

            string uniqueColumnWhere = "";

            // Get unique column WHERE condition
            if (!string.IsNullOrEmpty(uniqueColumn))
            {
                // Ensure string value
                uniqueColumnValue = SqlHelper.EscapeQuotes(uniqueColumnValue);

                uniqueColumnWhere = string.Format("{0} = N'{1}'", uniqueColumn, uniqueColumnValue);
                if (siteWhere != "")
                {
                    uniqueColumnWhere += " AND ";
                }
            }

            // Get complete WHERE condition
            return uniqueColumnWhere + siteWhere;
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(SiteInfo info)
        {
            if (info == null)
            {
                return;
            }

            LicenseHelper.ClearLicenseLimitation();

            if (info.SiteDefaultEditorStylesheet <= 0)
            {
                info.SetValue("SiteDefaultEditorStylesheet", null);
            }

            bool isUpdate = (info.SiteID > 0);

            // If exists, update
            if (isUpdate)
            {
                // Update hashtable in case the site code name has been changed
                SiteInfo si = GetInfoById(info.SiteID);
                if (si != null)
                {
                    // Remove from hash 
                    si.Status = info.Status;
                }

                base.SetInfo(info);
            }
            // Else insert a new record
            else
            {
                // Finds whether the given site name is unique
                bool siteExists;

                try
                {
                    SiteInfo siteInfo = GetInfoByCodeName(info.SiteName);
                    siteExists = (siteInfo != null);
                }
                catch
                {
                    siteExists = false;
                }

                // If site exists
                if (siteExists)
                {
                    throw new InvalidOperationException("[SiteInfoProvider.SetSiteInfo]: Site '" + info.SiteName + "' already exists.");
                }

                // Create a new record
                base.SetInfo(info);

                // Log event
                if (info.TypeInfo.LogEvents)
                {
                    EventLogProvider.LogEvent(EventType.INFORMATION, "Site creation", "SITECREATION");
                }
            }

            // Refresh all site provider hashtables (flush on other web farm servers as well)
            ClearHashtables(true);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(SiteInfo info)
        {
            if (info == null)
            {
                return;
            }

            // Delete site
            base.DeleteInfo(info);

            // Remove dependencies in cache
            ProviderHelper.ClearHashtables(SettingsKeyInfo.OBJECT_TYPE, true);
            SiteDomainAliasInfoProvider.RemoveFromStaticCollection(info.SiteID);

            // Clear all hashtables
            ModuleManager.ClearHashtables();

            RunningSites.Clear();

            // Log event
            EventLogProvider.LogEvent(EventType.INFORMATION, "Site", "SITEDESTRUCTION", "Site '" + info.DisplayName + "' destruction");
        }


        /// <summary>
        /// Deletes site with all dependent objects
        /// </summary>
        /// <param name="siteDeletionSettings">Site deletion settings</param>
        /// <param name="progressLog">Progress log</param>
        [CanDisableLicenseCheck("DWDdpIngO4CfpbT91sLqQP3aVnp9ep5zND0Y6nxOOi5Znry7Yo4py6yPHbX5zN/GBjOLsNAbH6I6DVAp8123JQ==")]
        protected virtual void DeleteSiteInfoInternal(SiteDeletionSettings siteDeletionSettings, IProgress<SiteDeletionStatusMessage> progressLog)
        {
            var site = siteDeletionSettings.Site;
            if (site == null)
            {
                return;
            }

            try
            {
                using (new CMSActionContext { EnableSmartSearchIndexer = false, DeletePhysicalFiles = false })
                {
                    // Handle the event
                    using (var h = SiteEvents.Delete.StartEvent(siteDeletionSettings, progressLog))
                    {
                        if (h.CanContinue())
                        {
                            LogMessage(progressLog, LogStatusEnum.Info, ResHelper.GetAPIString("Site_Delete.DeletingRest", "Deleting other site objects"));

                            // Delete other dependencies and the site
                            DeleteInfo(site);

                            // Delete meta files
                            if (siteDeletionSettings.DeleteMetaFiles)
                            {
                                var path = MetaFileInfoProvider.GetFilesFolderPath(site.SiteName);

                                LogMessage(progressLog, LogStatusEnum.Info, ResHelper.GetAPIString("Site_Delete.DeletingMetaFiles", "Deleting files folder"));

                                try
                                {
                                    if (Directory.Exists(path))
                                    {
                                        WebFarmHelper.CreateIOTask(StorageTaskType.DeleteFolder, path, null, "deletefolder", path.Substring(SystemContext.WebApplicationPhysicalPath.Length).TrimStart('\\'));
                                        DirectoryHelper.DeleteDirectory(path, true);
                                    }
                                }
                                catch
                                {
                                    LogMessage(progressLog, LogStatusEnum.Warning, string.Format(ResHelper.GetAPIString("Site_Delete.DeletingMetaFilesWarning", "Error deleting folder '{0}'. Please delete the folder manually."), path));
                                }
                            }
                        }

                        h.FinishEvent();
                    }

                    LogMessage(progressLog, LogStatusEnum.Finish, string.Format(ResHelper.GetAPIString("Site_Delete.DeletionFinished", "Site '{0}' has been successfully deleted.<br />"), HTMLHelper.HTMLEncode(ResHelper.LocalizeString(site.DisplayName))));
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                LogMessage(progressLog, LogStatusEnum.Error, string.Format(ResHelper.GetAPIString("Site_Delete.DeletionError", "Error deleting site: {0}"), EventLogProvider.GetExceptionLogMessage(ex)));
            }
        }


        private void LogMessage(IProgress<SiteDeletionStatusMessage> progressLog, LogStatusEnum status, string message)
        {
            if (progressLog == null)
            {
                return;
            }

            progressLog.Report(new SiteDeletionStatusMessage
            {
                Status = status,
                Message = message
            });
        }


        /// <summary>
        /// Sets the specified Site running.
        /// </summary>
        /// <param name="siteName">Site name of the site to run</param>
        protected virtual void RunSiteInternal(string siteName)
        {
            // Get the site
            SiteInfo si = GetInfoByCodeName(siteName);
            if ((si == null) || (si.Status == SiteStatusEnum.Running))
            {
                return;
            }

            // Check out if all the domains that the site is about to use are not running
            DataSet runningDS = CheckDomainNameForCollision(si.DomainName, si.SiteID);

            if (!DataHelper.DataSourceIsEmpty(runningDS))
            {
                SiteInfo runningsi = GetSiteInfo(ValidationHelper.GetInteger(runningDS.Tables[0].Rows[0]["SiteID"], 0));
                throw new RunningSiteException("There is already running site '" + HTMLHelper.HTMLEncode(runningsi.DisplayName) + "' with domain alias '" + HTMLHelper.HTMLEncode(ValidationHelper.GetString(runningDS.Tables[0].Rows[0]["SiteDomainAliasName"], "")) + "', you need to stop the site before running the site '" + HTMLHelper.HTMLEncode(si.DisplayName) + "'.");
            }

            // Run the site
            si.Status = SiteStatusEnum.Running;
            SetSiteInfo(si);
        }


        /// <summary>
        /// Returns site name from URL.
        /// </summary>
        /// <param name="url">URL of the site</param>
        protected virtual string GetSiteNameFromUrlInternal(string url)
        {
            string domain = URLHelper.GetDomain(url);
            if (String.IsNullOrEmpty(domain))
            {
                return null;
            }

            string appPath = SystemContext.ApplicationPath;

            SiteInfo siteInfo = GetRunningSiteInfo(domain, appPath);
            if (siteInfo != null)
            {
                return siteInfo.SiteName;
            }

            return null;
        }

        #endregion


        #region "Resources & Classes"

        /// <summary>
        /// Returns dataSet with document type classes of specified site.
        /// </summary>
        /// <param name="siteId">ID of site</param>
        protected virtual InfoDataSet<DataClassInfo> GetDocumentTypeClassPerSiteInternal(int siteId)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteId);

            parameters.FillDataSet = new InfoDataSet<DataClassInfo>();

            return ConnectionHelper.ExecuteQuery("cms.site.selectclasses", parameters).As<DataClassInfo>();
        }

        #endregion


        #region "Hashtable operations"

        /// <summary>
        /// Clear hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            if (logTasks)
            {
                CreateWebFarmTask(CLEAR_SITE_ACTION_NAME, null);
            }

            RunningSites.Clear();
            ProviderHelper.ClearHashtables(SiteDomainAliasInfo.OBJECT_TYPE, logTasks);
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Checks the specified domain name(and site) for collision with another running site.
        /// </summary>
        /// <param name="domainName">Domain name to be checked</param>
        /// <param name="siteId">ID of the site which holds the domain name (site is excluded from result)</param>
        /// <returns>Dataset with data of the first site which is running under specified domain name.</returns>
        public static DataSet CheckDomainNameForCollision(string domainName, int siteId)
        {
            if (!String.IsNullOrEmpty(domainName))
            {
                if (domainName.EndsWith(":80", StringComparison.Ordinal))
                {
                    domainName = domainName.Remove(domainName.LastIndexOf(":", StringComparison.Ordinal));
                }

                // Prepare the parameters
                var parameters = new QueryDataParameters();
                parameters.Add("@SiteID", siteId);
                parameters.Add("@DomainName", domainName);

                // Check out if all the domains that the site is about to use are not running
                return ConnectionHelper.ExecuteQuery("cms.site.selectrunningdomainnames", parameters);
            }

            return null;
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
                // Clear search indexes
                case CLEAR_SITE_ACTION_NAME:
                    ClearHashtables(false);
                    break;

                // If action name is not handled throw an exception
                default:
                    throw new Exception("[" + TypeInfo.ObjectType + ".ProcessWebFarmTask] The action name '" + actionName + "' has no supporting code.");
            }
        }

        #endregion
    }
}