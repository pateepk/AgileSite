using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing PublicStatusInfo management.
    /// </summary>
    public class PublicStatusInfoProvider : AbstractInfoProvider<PublicStatusInfo, PublicStatusInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public PublicStatusInfoProvider()
            : base(PublicStatusInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Name = true,
					Load = LoadHashtableEnum.All
				})
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all public statuses.
        /// </summary>
        public static ObjectQuery<PublicStatusInfo> GetPublicStatuses()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns public status with specified ID.
        /// </summary>
        /// <param name="statusId">Public status ID</param>        
        public static PublicStatusInfo GetPublicStatusInfo(int statusId)
        {
            return ProviderObject.GetInfoById(statusId);
        }


        /// <summary>
        /// Returns public status with specified name.
        /// </summary>
        /// <param name="statusName">Public status name</param>                
        /// <param name="siteName">Site name</param>                
        public static PublicStatusInfo GetPublicStatusInfo(string statusName, string siteName)
        {
            return ProviderObject.GetPublicStatusInfoInternal(statusName, siteName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified public status.
        /// </summary>
        /// <param name="statusObj">Public status to be set</param>
        public static void SetPublicStatusInfo(PublicStatusInfo statusObj)
        {
            ProviderObject.SetInfo(statusObj);
        }


        /// <summary>
        /// Deletes specified public status.
        /// </summary>
        /// <param name="statusObj">Public status to be deleted</param>
        public static void DeletePublicStatusInfo(PublicStatusInfo statusObj)
        {
            ProviderObject.DeleteInfo(statusObj);
        }


        /// <summary>
        /// Deletes public status with specified ID.
        /// </summary>
        /// <param name="statusId">Public status ID</param>
        public static void DeletePublicStatusInfo(int statusId)
        {
            var statusObj = GetPublicStatusInfo(statusId);
            DeletePublicStatusInfo(statusObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns dataset of all public statuses matching the specified parameters.
        /// </summary>
        /// <param name="siteId">ID of the site the public statuses should be retrieved from. If set to 0, global public statuses are retrieved</param>
        /// <param name="onlyEnabled">True - only enabled public statuses from the specified site are returned. False - all site public statuses are returned</param>
        public static ObjectQuery<PublicStatusInfo> GetPublicStatuses(int siteId, bool onlyEnabled = false)
        {
            return ProviderObject.GetPublicStatusesInternal(siteId, onlyEnabled);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns public status with specified name.
        /// </summary>
        /// <param name="statusName">Public status name</param>                
        /// <param name="siteName">Site name</param>         
        protected virtual PublicStatusInfo GetPublicStatusInfoInternal(string statusName, string siteName)
        {
            // Ensure site ID 
            int siteId = ECommerceHelper.GetSiteID(siteName, ECommerceSettings.USE_GLOBAL_PUBLIC_STATUS);

            return GetInfoByCodeName(statusName, siteId, true);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns dataset of all public statuses matching the specified parameters.
        /// </summary>
        /// <param name="siteId">ID of the site the statuses should be retrieved from. If set to 0, global public statuses are retrieved</param>
        /// <param name="onlyEnabled">True - only enabled statuses from the specified site are returned. False - all site statuses are returned</param>
        protected virtual ObjectQuery<PublicStatusInfo> GetPublicStatusesInternal(int siteId, bool onlyEnabled)
        {
            // Check if site uses site or global public statuses
            siteId = ECommerceHelper.GetSiteID(siteId, ECommerceSettings.USE_GLOBAL_PUBLIC_STATUS);

            // Get public statuses on requested site
            var query = GetPublicStatuses().OnSite(siteId);

            if (onlyEnabled)
            {
                query.WhereTrue("PublicStatusEnabled");
            }

            return query;
        }

        #endregion
    }
}