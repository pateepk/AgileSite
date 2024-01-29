using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing InternalStatusInfo management.
    /// </summary>
    public class InternalStatusInfoProvider : AbstractInfoProvider<InternalStatusInfo, InternalStatusInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public InternalStatusInfoProvider()
            : base(InternalStatusInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Name = true
				})
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all internal statuses.
        /// </summary>
        public static ObjectQuery<InternalStatusInfo> GetInternalStatuses()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns internal status with specified ID.
        /// </summary>
        /// <param name="statusId">Internal status ID</param>        
        public static InternalStatusInfo GetInternalStatusInfo(int statusId)
        {
            return ProviderObject.GetInfoById(statusId);
        }


        /// <summary>
        /// Returns internal status with specified name.
        /// </summary>
        /// <param name="statusName">Internal status name</param>                
        /// <param name="siteName">Site name</param>                
        public static InternalStatusInfo GetInternalStatusInfo(string statusName, string siteName)
        {
            return ProviderObject.GetInternalStatusInfoInternal(statusName, siteName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified internal status.
        /// </summary>
        /// <param name="statusObj">Internal status to be set</param>
        public static void SetInternalStatusInfo(InternalStatusInfo statusObj)
        {
            ProviderObject.SetInfo(statusObj);
        }


        /// <summary>
        /// Deletes specified internal status.
        /// </summary>
        /// <param name="statusObj">Internal status to be deleted</param>
        public static void DeleteInternalStatusInfo(InternalStatusInfo statusObj)
        {
            ProviderObject.DeleteInfo(statusObj);
        }


        /// <summary>
        /// Deletes internal status with specified ID.
        /// </summary>
        /// <param name="statusId">Internal status ID</param>
        public static void DeleteInternalStatusInfo(int statusId)
        {
            var statusObj = GetInternalStatusInfo(statusId);
            DeleteInternalStatusInfo(statusObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns dataset of all internal statuses for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param> 
        /// <param name="onlyEnabled">True - only enable internal statuses are returned.
        /// False - both enabled and disabled internal statuses are returned.</param>
        public static ObjectQuery<InternalStatusInfo> GetInternalStatuses(int siteId, bool onlyEnabled = false)
        {
            return ProviderObject.GetInternalStatusesInternal(siteId, onlyEnabled);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns internal status with specified name.
        /// </summary>
        /// <param name="statusName">Internal status name</param>                
        /// <param name="siteName">Site name</param>         
        protected virtual InternalStatusInfo GetInternalStatusInfoInternal(string statusName, string siteName)
        {
            // Ensure site ID 
            int siteId = ECommerceHelper.GetSiteID(siteName, ECommerceSettings.USE_GLOBAL_INTERNAL_STATUS);

            return GetInfoByCodeName(statusName, siteId, true);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns dataset of all internal statuses for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param> 
        /// <param name="onlyEnabled">True - only enable internal statuses are returned.
        /// False - both enabled and disabled internal statuses are returned.</param>
        protected virtual ObjectQuery<InternalStatusInfo> GetInternalStatusesInternal(int siteId, bool onlyEnabled)
        {
            // Check if site uses site or global internal statuses
            siteId = ECommerceHelper.GetSiteID(siteId, ECommerceSettings.USE_GLOBAL_INTERNAL_STATUS);

            // Get internal statuses on requested site
            var query = GetInternalStatuses().OnSite(siteId);

            if (onlyEnabled)
            {
                query.WhereTrue("InternalStatusEnabled");
            }

            return query;
        }

        #endregion
    }
}