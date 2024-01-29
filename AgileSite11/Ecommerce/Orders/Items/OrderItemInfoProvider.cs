using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Query;

namespace CMS.Ecommerce
{
    using TypedDataSet = InfoDataSet<OrderItemInfo>;

    /// <summary>
    /// Class providing OrderItemInfo management.
    /// </summary>
    public class OrderItemInfoProvider : AbstractInfoProvider<OrderItemInfo, OrderItemInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all order items.
        /// </summary>
        public static ObjectQuery<OrderItemInfo> GetOrderItems()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns order item with specified ID.
        /// </summary>
        /// <param name="itemId">Order item ID</param>        
        public static OrderItemInfo GetOrderItemInfo(int itemId)
        {
            return ProviderObject.GetOrderItemInfoInternal(itemId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified order item.
        /// </summary>
        /// <param name="itemObj">Order item to be set</param>
        public static void SetOrderItemInfo(OrderItemInfo itemObj)
        {
            ProviderObject.SetOrderItemInfoInternal(itemObj);
        }


        /// <summary>
        /// Deletes specified order item.
        /// </summary>
        /// <param name="itemObj">Order item to be deleted</param>
        public static void DeleteOrderItemInfo(OrderItemInfo itemObj)
        {
            ProviderObject.DeleteOrderItemInfoInternal(itemObj);
        }


        /// <summary>
        /// Deletes specified order item. Allows to return ordered items back to inventory.
        /// </summary>
        /// <param name="itemObj">Order item to be deleted</param>
        /// <param name="updateInventory">When true, items are returned back to inventory.</param>
        public static void DeleteOrderItemInfo(OrderItemInfo itemObj, bool updateInventory)
        {
            ProviderObject.DeleteOrderItemInfoInternal(itemObj, updateInventory);
        }


        /// <summary>
        /// Deletes order item with specified ID.
        /// </summary>
        /// <param name="itemId">Order item ID</param>
        public static void DeleteOrderItemInfo(int itemId)
        {
            var itemObj = GetOrderItemInfo(itemId);
            DeleteOrderItemInfo(itemObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns order item created form shopping cart item data.
        /// </summary>
        /// <param name="item">Shopping cart item.</param>
        public static OrderItemInfo GetOrderItemInfo(ShoppingCartItemInfo item)
        {
            return ProviderObject.GetOrderItemInfoInternal(item);
        }


        /// <summary>
        /// Returns all items of the specified order. Returned items contain pre-loaded <see cref="OrderItemInfo.OrderItemSKU"/> SKU data.
        /// </summary>
        /// <param name="orderId">Order ID</param>
        public static ICollection<OrderItemInfo> GetOrderItems(int orderId)
        {
            return ProviderObject.GetOrderItemsInternal(orderId);
        }


        /// <summary>
        /// Returns dataset of all expiring order items matching given parameters along with additional relevant information.
        /// </summary>
        /// <param name="days">Number of days before order item expiration</param>
        /// <param name="siteId">Site ID. Set to 0 to get expiring order items on all sites.</param>
        /// <param name="where">Additional where condition</param>
        /// <param name="onlyWithSendNotification">Get only records with send notification flag set to true.</param>
        public static TypedDataSet GetExpiringOrderItems(int days, int siteId, string where, bool onlyWithSendNotification = false)
        {
            return ProviderObject.GetExpiringOrderItemsInternal(days, siteId, where, onlyWithSendNotification);
        }


        /// <summary>
        /// Delete all order items for specified order.
        /// </summary>
        /// <param name="orderId">Order ID</param>
        public static void DeleteOrderItems(int orderId)
        {
            ProviderObject.DeleteOrderItemsInternal(orderId);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns order item with specified ID.
        /// </summary>
        /// <param name="itemId">Order item ID</param>        
        protected virtual OrderItemInfo GetOrderItemInfoInternal(int itemId)
        {
            return GetInfoById(itemId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified order item.
        /// </summary>
        /// <param name="itemObj">Order item to be set</param>        
        protected virtual void SetOrderItemInfoInternal(OrderItemInfo itemObj)
        {
            SetInfo(itemObj);
        }


        /// <summary>
        /// Deletes specified order item.
        /// </summary>
        /// <param name="itemObj">Order item to be deleted</param>        
        protected virtual void DeleteOrderItemInfoInternal(OrderItemInfo itemObj)
        {
            DeleteOrderItemInfoInternal(itemObj, false);
        }


        /// <summary>
        /// Deletes specified order item. Allows to return ordered items back to inventory.
        /// </summary>
        /// <param name="itemObj">Order item to be deleted.</param>
        /// <param name="updateInventory">When true, items are returned back to inventory.</param>
        protected virtual void DeleteOrderItemInfoInternal(OrderItemInfo itemObj, bool updateInventory)
        {
            if (updateInventory)
            {
                // Get SKU ordered by deleted order item
                SKUInfo sku = SKUInfoProvider.GetSKUInfo(itemObj.OrderItemSKUID);
                if (sku != null)
                {
                    // Sku is variant and track by product is enabled
                    if (sku.IsProductVariant && (sku.SKUTrackInventory == TrackInventoryTypeEnum.ByProduct))
                    {
                        // Get parent product of variant
                        SKUInfo parent = sku.Parent as SKUInfo;

                        if (parent != null)
                        {
                            // Update parent inventory
                            parent.SKUAvailableItems += itemObj.OrderItemUnitCount;
                            parent.Update();
                        }
                    }
                    else
                    {
                        // Update inventory
                        sku.SKUAvailableItems += itemObj.OrderItemUnitCount;
                        sku.Update();
                    }
                }
            }

            // Delete OrderItem
            DeleteInfo(itemObj);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns order item created from shopping cart item data.
        /// </summary>
        /// <param name="item">Shopping cart item.</param>
        protected virtual OrderItemInfo GetOrderItemInfoInternal(ShoppingCartItemInfo item)
        {
            // Do not process
            if ((item?.ShoppingCart == null) || (item.SKU == null))
            {
                return null;
            }

            var siteID = item.ShoppingCart.ShoppingCartSiteID;
            var mainCurrency = Service.Resolve<ISiteMainCurrencySource>().GetSiteMainCurrencyCode(siteID);

            // Set main properties
            OrderItemInfo oItem = new OrderItemInfo
            {
                OrderItemOrderID = item.ShoppingCart.OrderId,
                OrderItemSKUID = item.SKUID,
                OrderItemSKUName = item.SKU.SKUName,
                OrderItemUnitCount = item.CartItemUnits,
                OrderItemUnitPrice = item.UnitPrice,
                OrderItemTotalPrice = item.TotalPrice,
                OrderItemTotalPriceInMainCurrency = CurrencyConverter.Convert(item.TotalPrice, item.ShoppingCart.Currency.CurrencyCode, mainCurrency, siteID),
                OrderItemGUID = item.CartItemGUID,
                OrderItemParentGUID = item.CartItemParentGUID,
                OrderItemBundleGUID = item.CartItemBundleGUID,
                OrderItemText = item.CartItemText,
                OrderItemSKU = item.SKU
            };

            // Set custom data
            oItem.OrderItemCustomData.LoadData(item.CartItemCustomData.GetData());

            if (item.UnitDiscountSummary != null)
            {
                oItem.OrderItemProductDiscounts = item.UnitDiscountSummary.GetSummaryXml();
            }

            if (item.DiscountSummary != null)
            {
                oItem.OrderItemDiscountSummary = item.DiscountSummary.GetSummaryXml();
            }

            // Set custom fields
            foreach (string columnName in item.ColumnNames)
            {
                oItem.SetValue(columnName, item.GetValue(columnName));
            }

            return oItem;
        }


        /// <summary>
        /// Returns all items of the specified order. Returned items contain pre-loaded <see cref="OrderItemInfo.OrderItemSKU"/> SKU data.
        /// </summary>
        /// <param name="orderId">Order ID</param>
        protected virtual ICollection<OrderItemInfo> GetOrderItemsInternal(int orderId)
        {
            var itemsDs = new ObjectQuery<OrderItemInfo>()
                    .From(new QuerySource("COM_OrderItem")
                        .Join("COM_SKU", "COM_SKU.SKUID", "COM_OrderItem.OrderItemSKUID"))
                    .WhereEquals("OrderItemOrderID", orderId)
                .Result;

            var items = itemsDs.Tables[0].AsEnumerable().Select(dr =>
            {
                var item = new OrderItemInfo(dr);
                var sku = new SKUInfo(dr);
                item.OrderItemSKU = sku;

                return item;
            });

            return items.ToList();
        }


        /// <summary>
        /// Returns dataset of all expiring order items matching given parameters along with additional relevant information.
        /// </summary>
        /// <param name="days">Number of days before order item expiration</param>
        /// <param name="siteId">Site ID. Set to 0 to get expiring order items on all sites.</param>
        /// <param name="where">Additional where condition</param>
        /// <param name="onlyWithSendNotification">Get only records with send notification flag set to true.</param>
        protected virtual TypedDataSet GetExpiringOrderItemsInternal(int days, int siteId, string where, bool onlyWithSendNotification)
        {
            // Set query parameters
            var queryParams = new QueryDataParameters
            {
                { "@Days", days },
                { "@SiteID", siteId },
                { "@OnlyWithSendNotification", onlyWithSendNotification }
            };
            queryParams.EnsureDataSet<OrderItemInfo>();

            return ConnectionHelper.ExecuteQuery("ecommerce.orderitem.selectexpiring", queryParams, where).As<OrderItemInfo>();
        }


        /// <summary>
        /// Delete all order items for specified order.
        /// </summary>
        /// <param name="orderId">Order ID</param>
        protected virtual void DeleteOrderItemsInternal(int orderId)
        {
            // Get all order items
            var items = GetOrderItems(orderId);

            // Delete them all
            foreach (var item in items)
            {
                DeleteOrderItemInfo(item);
            }
        }

        #endregion
    }
}