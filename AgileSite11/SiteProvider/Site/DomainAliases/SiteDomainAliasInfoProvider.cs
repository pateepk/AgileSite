using System;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.SiteProvider
{
    /// <summary>
    /// Class providing SiteDomainAliasInfo management.
    /// </summary>
    public class SiteDomainAliasInfoProvider : AbstractInfoProvider<SiteDomainAliasInfo, SiteDomainAliasInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public SiteDomainAliasInfoProvider()
            : base(SiteDomainAliasInfo.TYPEINFO, new HashtableSettings
            {
                ID = true
            })
        {
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        /// <param name="siteId">Site ID</param>
        public static SiteDomainAliasInfo GetSiteDomainAliasInfoByGUID(Guid guid, int siteId)
        {
            return ProviderObject.GetSiteDomainAliasInfoByGUIDInternal(guid, siteId);
        }


        /// <summary>
        /// Returns the SiteDomainAliasInfo structure for the specified site domain alias ID.
        /// </summary>
        /// <param name="domainAliasId">Site domain alias id</param>
        public static SiteDomainAliasInfo GetSiteDomainAliasInfo(int domainAliasId)
        {
            return ProviderObject.GetInfoById(domainAliasId);
        }


        /// <summary>
        /// Returns the SiteDomainAliasInfo structure for the specified site domain alias name.
        /// </summary>
        /// <param name="domainAlias">Site domain alias name</param>
        /// <param name="siteId">Site ID</param>
        public static SiteDomainAliasInfo GetSiteDomainAliasInfo(string domainAlias, int siteId)
        {
            return ProviderObject.GetSiteDomainAliasInfoInternal(domainAlias, siteId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified site domain alias.
        /// </summary>
        /// <param name="siteDomain">SiteDomainAliasInfo object to set</param>
        /// <param name="originalDomainAlias">Original domain alias name</param>
        public static void SetSiteDomainAliasInfo(SiteDomainAliasInfo siteDomain, string originalDomainAlias)
        {
            ProviderObject.SetSiteDomainAliasInfoInternal(siteDomain, originalDomainAlias);
        }


        /// <summary>
        /// Sets (updates or inserts) specified site domain alias.
        /// </summary>
        /// <param name="siteDomain">SiteDomainAliasInfo object to set</param>
        public static void SetSiteDomainAliasInfo(SiteDomainAliasInfo siteDomain)
        {
            SetSiteDomainAliasInfo(siteDomain, null);
        }


        /// <summary>
        /// Deletes specified site domain alias.
        /// </summary>
        /// <param name="siteDomainId">Site domain alias id</param>
        public static void DeleteSiteDomainAliasInfo(int siteDomainId)
        {
            SiteDomainAliasInfo siteDomainObj = GetSiteDomainAliasInfo(siteDomainId);
            DeleteSiteDomainAliasInfo(siteDomainObj);
        }


        /// <summary>
        /// Deletes specified site domain alias.
        /// </summary>
        /// <param name="siteDomainObj">Site domain alias object</param>
        public static void DeleteSiteDomainAliasInfo(SiteDomainAliasInfo siteDomainObj)
        {
            ProviderObject.DeleteInfo(siteDomainObj);
        }


        /// <summary>
        /// Returns all site domain aliases assigned to the selected site.
        /// </summary>
        /// <param name="siteId">Site ID</param>     
        public static InfoDataSet<SiteDomainAliasInfo> GetDomainAliases(int siteId)
        {
            return ProviderObject.GetDomainAliasesInternal(siteId);
        }


        /// <summary>
        /// Returns all site domain aliases assigned to the selected site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static InfoDataSet<SiteDomainAliasInfo> GetDomainAliases(string siteName)
        {
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si != null)
            {
                return GetDomainAliases(si.SiteID);
            }

            return null;
        }


        /// <summary>
        /// Returns all domain aliases.
        /// </summary>
        public static ObjectQuery<SiteDomainAliasInfo> GetDomainAliases()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Checks if domain alias exists for specified site.
        /// </summary>
        /// <param name="domainAlias">Domain alias</param>
        /// <param name="siteId">Site ID</param>
        public static bool DomainAliasExists(string domainAlias, int siteId)
        {
            return (GetSiteDomainAliasInfo(domainAlias, siteId) != null);
        }


        /// <summary>
        /// Deletes the site domain aliases.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static void DeleteSiteAliases(int siteId)
        {
            // Get the aliases
            var siteAliases = GetDomainAliases(siteId);
            if (!DataHelper.DataSourceIsEmpty(siteAliases))
            {
                foreach (var siteAlias in siteAliases)
                {
                    // Delete the alias
                    ProviderObject.DeleteInfo(siteAlias);
                }

                ProviderHelper.ClearHashtables(SiteInfo.OBJECT_TYPE, true);
            }
        }


        /// <summary>
        /// Removes domain aliases for specified site from cached collection
        /// </summary>
        internal static void RemoveFromStaticCollection(int siteId)
        {
            if (DomainAliasesBySiteId != null)
            {
                DomainAliasesBySiteId[siteId] = null;
            }
        }


        /// <summary>
        /// Clear hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            DomainAliasesBySiteId.Clear();
        }

        #endregion


        #region "Private fields"

        /// <summary>
        /// Table of the domain aliases DataSets indexed by site ID. [siteId -> InfoDataSet]
        /// </summary>
        private static readonly CMSStatic<SafeDictionary<int, InfoDataSet<SiteDomainAliasInfo>>> mDomainAliasesBySiteId = new CMSStatic<SafeDictionary<int, InfoDataSet<SiteDomainAliasInfo>>>(() => new SafeDictionary<int, InfoDataSet<SiteDomainAliasInfo>>());


        /// <summary>
        /// Table of the domain aliases DataSets indexed by site ID. [siteId -> InfoDataSet]
        /// </summary>
        private static SafeDictionary<int, InfoDataSet<SiteDomainAliasInfo>> DomainAliasesBySiteId
        {
            get
            {
                return mDomainAliasesBySiteId;
            }
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        /// <param name="siteId">Site ID</param>
        protected virtual SiteDomainAliasInfo GetSiteDomainAliasInfoByGUIDInternal(Guid guid, int siteId)
        {
            return GetObjectQuery().TopN(1)
                .WhereEquals("SiteDomainGUID", guid)
                .WhereEquals("SiteID", siteId).FirstOrDefault();
        }


        /// <summary>
        /// Returns the SiteDomainAliasInfo structure for the specified site domain alias name.
        /// </summary>
        /// <param name="domainAlias">Site domain alias name</param>
        /// <param name="siteId">Site ID</param>
        protected virtual SiteDomainAliasInfo GetSiteDomainAliasInfoInternal(string domainAlias, int siteId)
        {
            return GetObjectQuery().TopN(1)
                .WhereEquals("SiteDomainAliasName", domainAlias)
                .WhereEquals("SiteID", siteId).FirstOrDefault();
        }


        /// <summary>
        /// Sets (updates or inserts) specified site domain alias.
        /// </summary>
        /// <param name="siteDomain">SiteDomainAliasInfo object to set</param>
        /// <param name="originalDomainAlias">Original domain alias name</param>
        protected virtual void SetSiteDomainAliasInfoInternal(SiteDomainAliasInfo siteDomain, string originalDomainAlias)
        {
            LicenseHelper.ClearLicenseLimitation();

            if (siteDomain != null)
            {
                // Get site info
                SiteInfo si = SiteInfoProvider.GetSiteInfo(siteDomain.SiteID);

                // If site info doesn't exist throw exception
                if (si == null)
                {
                    throw new Exception("[SiteDomainAliasInfoProvider.SetSiteDomainAliasInfo]: Site object for specified site domain '" + siteDomain.SiteDomainAliasName + "' was not found.");
                }

                // Do not validate code name due to allowed special characters in domain name
                siteDomain.Generalized.ValidateCodeName = false;

                // Set the Site domain alias object
                SetInfo(siteDomain);

                // SiteInfoProvider clears SiteDomainAlias hash tables as well
                ProviderHelper.ClearHashtables(SiteInfo.OBJECT_TYPE, true);

                RemoveFromStaticCollection(si.SiteID);
            }
            else
            {
                throw new Exception("[SiteDomainAliasInfoProvider.SetSiteDomainAliasInfo]: No SiteDomainAliasInfo object set.");
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(SiteDomainAliasInfo info)
        {
            if (info != null)
            {
                base.DeleteInfo(info);

                ProviderHelper.ClearHashtables(SiteInfo.OBJECT_TYPE, true);
            }
        }


        /// <summary>
        /// Returns all site domain aliases assigned to the selected site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual InfoDataSet<SiteDomainAliasInfo> GetDomainAliasesInternal(int siteId)
        {
            if (DomainAliasesBySiteId[siteId] != null)
            {
                return DomainAliasesBySiteId[siteId];
            }

            var ds = GetObjectQuery().WhereEquals("SiteID", siteId).TypedResult;
            DomainAliasesBySiteId[siteId] = ds;

            return ds;
        }

        #endregion
    }
}