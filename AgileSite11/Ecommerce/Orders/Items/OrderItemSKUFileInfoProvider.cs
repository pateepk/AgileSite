using System;

using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Ecommerce
{
    using TypedDataSet = InfoDataSet<OrderItemSKUFileInfo>;

    /// <summary>
    /// Class providing OrderItemSKUFileInfo management.
    /// </summary>
    public class OrderItemSKUFileInfoProvider : AbstractInfoProvider<OrderItemSKUFileInfo, OrderItemSKUFileInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all order item SKU files.
        /// </summary>
        public static ObjectQuery<OrderItemSKUFileInfo> GetOrderItemSKUFiles()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns order item SKU file with specified ID.
        /// </summary>
        /// <param name="fileId">Order item SKU file ID.</param>        
        public static OrderItemSKUFileInfo GetOrderItemSKUFileInfo(int fileId)
        {
            return ProviderObject.GetInfoById(fileId);
        }


        /// <summary>
        /// Returns order item SKU file with specified unique token.
        /// </summary>
        /// <param name="token">Order item SKU file unique token.</param>        
        public static OrderItemSKUFileInfo GetOrderItemSKUFileInfo(Guid token)
        {
            return ProviderObject.GetInfoByGuid(token);
        }


        /// <summary>
        /// Sets (updates or inserts) specified order item SKU file.
        /// </summary>
        /// <param name="fileObj">Order item SKU file to be set.</param>
        public static void SetOrderItemSKUFileInfo(OrderItemSKUFileInfo fileObj)
        {
            ProviderObject.SetInfo(fileObj);
        }


        /// <summary>
        /// Deletes specified order item SKU file.
        /// </summary>
        /// <param name="fileObj">Order item SKU file to be deleted.</param>
        public static void DeleteOrderItemSKUFileInfo(OrderItemSKUFileInfo fileObj)
        {
            ProviderObject.DeleteInfo(fileObj);
        }


        /// <summary>
        /// Deletes order item SKU file with specified ID.
        /// </summary>
        /// <param name="fileId">Order item SKU file ID.</param>
        public static void DeleteOrderItemSKUFileInfo(int fileId)
        {
            var fileObj = GetOrderItemSKUFileInfo(fileId);
            DeleteOrderItemSKUFileInfo(fileObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns dataset of all order item SKU files matching the specified parameters with additional details.
        /// </summary>
        /// <param name="orderId">Order ID</param>
        public static TypedDataSet GetOrderItemSKUFiles(int orderId)
        {
            return ProviderObject.GetOrderItemSKUFilesInternal(orderId);
        }


        /// <summary>
        /// Returns e-product download URL.
        /// </summary>
        /// <param name="token">File unique download token</param>
        /// <param name="fileName">File name</param>
        /// <param name="siteId">Site ID of the order in which the e-product is included</param>
        public static string GetOrderItemSKUFileUrl(Guid token, string fileName, int siteId)
        {
            SiteInfo site = SiteInfoProvider.GetSiteInfo(siteId);
            if (site != null)
            {
                string url = String.Format("~/GetProductFile/{0}/{1}.aspx", token, URLHelper.GetSafeFileName(fileName, site.SiteName));
                return URLHelper.GetAbsoluteUrl(url, site.DomainName);
            }

            return "";
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns dataset of all order item SKU files matching the specified parameters with additional details.
        /// </summary>
        /// <param name="orderId">Order ID</param>
        protected virtual TypedDataSet GetOrderItemSKUFilesInternal(int orderId)
        {
            var where = $"COM_OrderItemSKUFile.OrderItemID IN (SELECT OrderItemID FROM COM_OrderItem WHERE OrderItemOrderID = {orderId})";

            var parameters = new QueryDataParameters();
            parameters.EnsureDataSet<OrderItemSKUFileInfo>();

            return ConnectionHelper.ExecuteQuery("ecommerce.orderitemskufile.selectallwithdetails", parameters, where, "COM_OrderItemSKUFile.OrderItemID").As<OrderItemSKUFileInfo>();
        }

        #endregion
    }
}