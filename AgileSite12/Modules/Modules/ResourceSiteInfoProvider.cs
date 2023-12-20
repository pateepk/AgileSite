using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.Modules
{
    using BoolDictionary = SafeDictionary<string, bool>;
    using ResourceTable = SafeDictionary<string, SafeDictionary<string, bool>>;


    /// <summary>
    /// Class providing ResourceSiteInfo management.
    /// </summary>
    public class ResourceSiteInfoProvider : AbstractInfoProvider<ResourceSiteInfo, ResourceSiteInfoProvider>
    {
        #region "Private fields"

        /// <summary>
        /// Table of the resources assigned to site [siteName] -> [resourceName -> true]
        /// </summary>
        private static readonly CMSStatic<ResourceTable> mSiteResources = new CMSStatic<ResourceTable>();


        /// <summary>
        /// Table of the resources assigned to site [siteName] -> [resourceName -> true]
        /// </summary>
        internal static ResourceTable SiteResources
        {
            get
            {
                return mSiteResources;
            }
            set
            {
                mSiteResources.Value = value;
            }
        }

        private static object tableLock = new object();

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns all bindings between modules(resources) and sites.
        /// </summary>
        public static ObjectQuery<ResourceSiteInfo> GetResourceSites()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the ResourceSiteInfo structure for the specified resourceSite.
        /// </summary>
        /// <param name="resourceId">ResourceID</param>
        /// <param name="siteId">SiteID</param>
        public static ResourceSiteInfo GetResourceSiteInfo(int resourceId, int siteId)
        {
            return ProviderObject.GetResourceSiteInfoInternal(resourceId, siteId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified resourceSite.
        /// </summary>
        /// <param name="resourceSite">ResourceSite to set</param>
        public static void SetResourceSiteInfo(ResourceSiteInfo resourceSite)
        {
            ProviderObject.SetInfo(resourceSite);
        }


        /// <summary>
        /// Removes specified resource from site.
        /// </summary>
        /// <param name="infoObj">ResourceSite object</param>
        public static void DeleteResourceSiteInfo(ResourceSiteInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Removes specified resource from site.
        /// </summary>
        /// <param name="resourceId">ResourceID</param>
        /// <param name="siteId">SiteID</param>
        public static void RemoveResourceFromSite(int resourceId, int siteId)
        {
            ResourceSiteInfo infoObj = GetResourceSiteInfo(resourceId, siteId);
            DeleteResourceSiteInfo(infoObj);
        }


        /// <summary>
        /// Adds specified resource to the site.
        /// </summary>
        /// <param name="resourceId">ResourceID</param>
        /// <param name="siteId">SiteID</param>
        public static void AddResourceToSite(int resourceId, int siteId)
        {
            // Create new binding
            ResourceSiteInfo infoObj = new ResourceSiteInfo();
            infoObj.ResourceID = resourceId;
            infoObj.SiteID = siteId;

            // Save to the database
            SetResourceSiteInfo(infoObj);
        }


        /// <summary>
        /// Returns if the resource is available for specified site.
        /// </summary>
        /// <param name="resourceName">Name of the resource</param>
        /// <param name="siteName">Name of the site</param>
        public static bool IsResourceOnSite(string resourceName, string siteName)
        {
            return ProviderObject.IsResourceOnSiteInternal(resourceName, siteName);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Clear hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            if (SiteResources != null)
            {
                SiteResources.Clear();
            }
        }


        /// <summary>
        /// Returns the ResourceSiteInfo structure for the specified resourceSite.
        /// </summary>
        /// <param name="resourceId">ResourceID</param>
        /// <param name="siteId">SiteID</param>
        protected virtual ResourceSiteInfo GetResourceSiteInfoInternal(int resourceId, int siteId)
        {
            var condition = new WhereCondition()
                .WhereEquals("ResourceID", resourceId)
                .WhereEquals("SiteID", siteId);

            return GetObjectQuery().TopN(1).Where(condition).FirstOrDefault();
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ResourceSiteInfo info)
        {
            // Set the data
            base.SetInfo(info);

            // Clear hashtable
            ClearHashtables(false);

            using (new CMSActionContext { LogSynchronization = false })
            {
                // Synchronize with page types
                GetRelatedDocumentTypeIDs(info.ResourceID).ForEach(docTypeID => ClassSiteInfoProvider.AddClassToSite(docTypeID, info.SiteID));
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ResourceSiteInfo info)
        {
            // Delete the data
            base.DeleteInfo(info);

            // Clear hashtable
            ClearHashtables(false);

            using (new CMSActionContext { LogSynchronization = false })
            {
                // Synchronize with page types
                GetRelatedDocumentTypeIDs(info.ResourceID).ForEach(docTypeID => ClassSiteInfoProvider.RemoveClassFromSite(docTypeID, info.SiteID));
            }
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns if the resource is available for specified site.
        /// </summary>
        /// <param name="resourceName">Name of the resource</param>
        /// <param name="siteName">Name of the site</param>
        protected virtual bool IsResourceOnSiteInternal(string resourceName, string siteName)
        {
            // System resource is always on site
            if (ResourceInfoProvider.IsSystemResource(resourceName))
            {
                return true;
            }

            lock (tableLock)
            {
                if (SiteResources == null)
                {
                    SiteResources = new ResourceTable();
                }

                // Load resources for given site
                BoolDictionary resources = SiteResources[siteName.ToLowerCSafe()];
                if (resources == null)
                {
                    resources = new BoolDictionary();

                    // Get the resources for the give site
                    DataSet ds = ResourceInfoProvider.GetResources(SiteInfoProvider.GetSiteID(siteName));
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            string resName = ValidationHelper.GetString(dr["ResourceName"], "").ToLowerCSafe();
                            resources[resName] = true;
                        }
                    }

                    SiteResources[siteName.ToLowerCSafe()] = resources;
                }

                return resources.Contains(resourceName.ToLowerCSafe());
            }
        }


        /// <summary>
        /// Returns IDs of document types that are related to the module with given id.
        /// </summary>
        /// <param name="resourceID">Module ID</param>
        private static List<int> GetRelatedDocumentTypeIDs(int resourceID)
        {
            return DataClassInfoProvider.GetClasses()
                                        .Column("ClassID")
                                        .WhereEquals("ClassResourceID", resourceID)
                                        .WhereTrue("ClassIsDocumentType")
                                        .GetListResult<int>()
                                        .ToList();
        }

        #endregion
    }
}