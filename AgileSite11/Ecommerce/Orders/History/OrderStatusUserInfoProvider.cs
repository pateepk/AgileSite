using System;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing OrderStatusUserInfo management.
    /// </summary>
    public class OrderStatusUserInfoProvider : AbstractInfoProvider<OrderStatusUserInfo, OrderStatusUserInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all order status history items.
        /// </summary>
        public static ObjectQuery<OrderStatusUserInfo> GetOrderStatusHistory()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns order status history item with specified ID.
        /// </summary>
        /// <param name="itemId">Order status history item ID</param>        
        public static OrderStatusUserInfo GetOrderStatusUserInfo(int itemId)
        {
            return ProviderObject.GetInfoById(itemId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified order status history item.
        /// </summary>
        /// <param name="itemObj">Order status history item to be set</param>
        public static void SetOrderStatusUserInfo(OrderStatusUserInfo itemObj)
        {
            ProviderObject.SetInfo(itemObj);
        }


        /// <summary>
        /// Deletes specified order status history item.
        /// </summary>
        /// <param name="itemObj">Order status history item to be deleted</param>
        public static void DeleteOrderStatusUserInfo(OrderStatusUserInfo itemObj)
        {
            ProviderObject.DeleteInfo(itemObj);
        }


        /// <summary>
        /// Deletes order status history item with specified ID.
        /// </summary>
        /// <param name="itemId">Order status history item ID</param>
        public static void DeleteOrderStatusUserInfo(int itemId)
        {
            var itemObj = GetOrderStatusUserInfo(itemId);
            DeleteOrderStatusUserInfo(itemObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns the query containing order status history for specified order.
        /// </summary>
        /// <param name="orderId">Order ID</param>
        public static ObjectQuery<OrderStatusUserInfo> GetOrderStatusHistory(int orderId)
        {
            return ProviderObject.GetOrderStatusHistoryInternal(orderId);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns the query containing order status history for given orderId.
        /// </summary>
        /// <param name="orderId">ID of the order to get history for</param>
        protected virtual ObjectQuery<OrderStatusUserInfo> GetOrderStatusHistoryInternal(int orderId)
        {
            return GetOrderStatusHistory()
                       .Where("OrderID", QueryOperator.Equals, orderId)
                       .OrderByDescending("Date")
                       .OrderByDescending("OrderStatusUserID");
        }

        #endregion
    }
}