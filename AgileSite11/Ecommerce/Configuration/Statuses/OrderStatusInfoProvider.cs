using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing OrderStatusInfo management.
    /// </summary>
    public class OrderStatusInfoProvider : AbstractInfoProvider<OrderStatusInfo, OrderStatusInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public OrderStatusInfoProvider()
            : base(OrderStatusInfo.TYPEINFO, new HashtableSettings
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
        /// Returns the query for all order statuses.
        /// </summary>
        public static ObjectQuery<OrderStatusInfo> GetOrderStatuses()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns order status with specified ID.
        /// </summary>
        /// <param name="statusId">Order status ID</param>        
        public static OrderStatusInfo GetOrderStatusInfo(int statusId)
        {
            return ProviderObject.GetInfoById(statusId);
        }


        /// <summary>
        /// Returns order status with specified name.
        /// </summary>
        /// <param name="statusName">Order status name</param>                
        /// <param name="siteName">Site name</param>                
        public static OrderStatusInfo GetOrderStatusInfo(string statusName, string siteName)
        {
            return ProviderObject.GetOrderStatusInfoInternal(statusName, siteName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified order status.
        /// </summary>
        /// <param name="statusObj">Order status to be set</param>
        public static void SetOrderStatusInfo(OrderStatusInfo statusObj)
        {
            ProviderObject.SetInfo(statusObj);
        }


        /// <summary>
        /// Deletes specified order status.
        /// </summary>
        /// <param name="statusObj">Order status to be deleted</param>
        public static void DeleteOrderStatusInfo(OrderStatusInfo statusObj)
        {
            ProviderObject.DeleteInfo(statusObj);
        }


        /// <summary>
        /// Deletes order status with specified ID.
        /// </summary>
        /// <param name="statusId">Order status ID</param>
        public static void DeleteOrderStatusInfo(int statusId)
        {
            var statusObj = GetOrderStatusInfo(statusId);
            DeleteOrderStatusInfo(statusObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns dataset of all order statuses matching the specified parameters.
        /// </summary>
        /// <param name="siteId">ID of the site the order statuses should be retrieved from. If set to 0, global order statuses are retrieved</param>
        /// <param name="onlyEnabled">True - only enabled order statuses from the specified site are returned. False - all site order statuses are returned</param>
        public static ObjectQuery<OrderStatusInfo> GetOrderStatuses(int siteId, bool onlyEnabled = false)
        {
            return ProviderObject.GetOrderStatusesInternal(siteId, onlyEnabled);
        }


        /// <summary>
        /// Returns first enabled order status in order progress for given site.
        /// </summary>
        public static OrderStatusInfo GetFirstEnabledStatus(int siteId)
        {
            return ProviderObject.GetFirstEnabledStatusInternal(siteId);
        }


        /// <summary>
        /// Returns next enabled order status in order progress.
        /// </summary>
        /// <param name="statusId">Current order status id</param>
        public static OrderStatusInfo GetNextEnabledStatus(int statusId)
        {
            return ProviderObject.GetNextEnabledStatusInternal(statusId);
        }


        /// <summary>
        /// Returns previous enabled order status in order progress.
        /// </summary>
        /// <param name="statusId">Current order status id</param>
        public static OrderStatusInfo GetPreviousEnabledStatus(int statusId)
        {
            return ProviderObject.GetPreviousEnabledStatusInternal(statusId);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns order status with specified name.
        /// </summary>
        /// <param name="statusName">Order status name</param>                
        /// <param name="siteName">Site name</param>         
        protected virtual OrderStatusInfo GetOrderStatusInfoInternal(string statusName, string siteName)
        {
            // Ensure site ID 
            int siteId = ECommerceHelper.GetSiteID(siteName, ECommerceSettings.USE_GLOBAL_ORDER_STATUS);

            return GetInfoByCodeName(statusName, siteId, true);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns dataset of all order statuses matching the specified parameters.
        /// </summary>
        /// <param name="siteId">ID of the site the statuses should be retrieved from. If set to 0, global order statuses are retrieved</param>
        /// <param name="onlyEnabled">True - only enabled statuses from the specified site are returned. False - all site statuses are returned</param>
        protected virtual ObjectQuery<OrderStatusInfo> GetOrderStatusesInternal(int siteId, bool onlyEnabled)
        {
            // Check if site uses site or global public statuses
            siteId = ECommerceHelper.GetSiteID(siteId, ECommerceSettings.USE_GLOBAL_ORDER_STATUS);

            // Get public statuses on requested site
            var query = GetOrderStatuses().OnSite(siteId);

            if (onlyEnabled)
            {
                query.WhereTrue("StatusEnabled");
            }

            return query;
        }


        /// <summary>
        /// Returns first enabled order status in order progress.
        /// </summary>
        protected virtual OrderStatusInfo GetFirstEnabledStatusInternal(int siteId)
        {
            var status = CacheHelper.Cache( 
                () => GetOrderStatuses(siteId, true).TopN(1).OrderBy("StatusOrder").FirstOrDefault(), 
                new CacheSettings(ECommerceSettings.ProvidersCacheMinutes, "OrderStatusInfoProvider", "GetFirstEnabledStatus", siteId)
                {
                    GetCacheDependency = () => CacheHelper.GetCacheDependency(OrderStatusInfo.OBJECT_TYPE + "|all")
                });

            return status;
        }

      
        /// <summary>
        /// Returns next enabled status in order progress.
        /// </summary>
        /// <param name="statusId">Current order status id</param>
        protected virtual OrderStatusInfo GetNextEnabledStatusInternal(int statusId)
        {
            // Status to find next status for
            var currentStatus = GetOrderStatusInfo(statusId);

            if (currentStatus == null)
            {
                return null;
            }

            var nextStatus = GetObjectQuery().TopN(1)
                               .OnSite(currentStatus.StatusSiteID)
                               .Where("StatusOrder", QueryOperator.LargerThan, currentStatus.StatusOrder)
                               .WhereTrue("StatusEnabled")
                               .OrderBy("StatusOrder");

            return nextStatus.FirstOrDefault();
        }


        /// <summary>
        /// Returns previous enabled status in order progress.
        /// </summary>
        /// <param name="statusId">Current order status id</param>
        protected virtual OrderStatusInfo GetPreviousEnabledStatusInternal(int statusId)
        {
            // Status to find previous status for
            var currentStatus = GetOrderStatusInfo(statusId);

            if (currentStatus == null)
            {
                return null;
            }

            var previousStatus = GetObjectQuery().TopN(1)
                                   .OnSite(currentStatus.StatusSiteID)
                                   .Where("StatusOrder", QueryOperator.LessThan, currentStatus.StatusOrder)
                                   .WhereTrue("StatusEnabled")
                                   .OrderByDescending("StatusOrder");

            return previousStatus.FirstOrDefault();
        }

        #endregion

    }
}