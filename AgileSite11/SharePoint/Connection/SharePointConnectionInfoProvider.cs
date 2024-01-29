using System;

using CMS.DataEngine;
using CMS.SiteProvider;

namespace CMS.SharePoint
{
    /// <summary>
    /// Class providing SharePointConnectionInfo management.
    /// </summary>
    public class SharePointConnectionInfoProvider : AbstractInfoProvider<SharePointConnectionInfo, SharePointConnectionInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public SharePointConnectionInfoProvider()
            : base(SharePointConnectionInfo.TYPEINFO, new HashtableSettings { ID = true, Name = true, Load = LoadHashtableEnum.None })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the SharePointConnectionInfo objects.
        /// </summary>
        public static ObjectQuery<SharePointConnectionInfo> GetSharePointConnections()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns SharePointConnectionInfo with specified ID.
        /// </summary>
        /// <param name="id">SharePointConnectionInfo ID</param>
        public static SharePointConnectionInfo GetSharePointConnectionInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns SharePointConnectionInfo with specified name.
        /// </summary>
        /// <param name="name">SharePointConnectionInfo name</param>
        /// <param name="siteName">Site name</param>
        public static SharePointConnectionInfo GetSharePointConnectionInfo(string name, SiteInfoIdentifier siteName)
        {
            return ProviderObject.GetInfoByCodeName(name, SiteInfoProvider.GetSiteID(siteName));
        }


        /// <summary>
        /// Returns SharePointConnectionInfo with specified GUID.
        /// </summary>
        /// <param name="guid">SharePointConnectionInfo GUID</param>                
        public static SharePointConnectionInfo GetSharePointConnectionInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified SharePointConnectionInfo.
        /// </summary>
        /// <param name="infoObj">SharePointConnectionInfo to be set</param>
        public static void SetSharePointConnectionInfo(SharePointConnectionInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified SharePointConnectionInfo.
        /// </summary>
        /// <param name="infoObj">SharePointConnectionInfo to be deleted</param>
        public static void DeleteSharePointConnectionInfo(SharePointConnectionInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes SharePointConnectionInfo with specified ID.
        /// </summary>
        /// <param name="id">SharePointConnectionInfo ID</param>
        public static void DeleteSharePointConnectionInfo(int id)
        {
            SharePointConnectionInfo infoObj = GetSharePointConnectionInfo(id);
            DeleteSharePointConnectionInfo(infoObj);
        }

        #endregion


        #region "Public methods - Advanced"


        /// <summary>
        /// Returns a query for all the SharePointConnectionInfo objects of a specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static ObjectQuery<SharePointConnectionInfo> GetSharePointConnections(SiteInfoIdentifier siteId)
        {
            return ProviderObject.GetSharePointConnectionsInternal(siteId);
        }

        #endregion


        #region "Internal methods - Advanced"


        /// <summary>
        /// Returns a query for all the SharePointConnectionInfo objects of a specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual ObjectQuery<SharePointConnectionInfo> GetSharePointConnectionsInternal(SiteInfoIdentifier siteId)
        {
            return GetObjectQuery().OnSite(siteId);
        }

        #endregion
    }
}