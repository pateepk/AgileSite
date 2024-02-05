using System;
using System.Collections.Generic;
using System.Data;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Helper class that helps to map objects to Google Tag Manager order related objects.
    /// </summary>
    public class GtmOrderHelper : AbstractHelper<GtmOrderHelper>
    {
        /// <summary>
        /// Maps <see cref="OrderInfo"/> to <see cref="GtmData"/> representing Google Tag Manager purchase.
        /// Purchase consists of order and its items.
        /// </summary>
        /// <param name="order"><see cref="OrderInfo"/> to be mapped to Google Tag Manager purchase object.</param>
        /// <param name="additionalData">Data with additional non-conflicting key value pairs to be merged resulting <see cref="GtmData"/> object.</param>
        /// <param name="purpose">Contextual information fitting for customizations.</param>
        /// <returns>The <see cref="GtmData"/> that represents Google Tag Manger purchase object.</returns>
        /// <seealso cref="MapPurchaseInternal(OrderInfo, object, string)"/>
        /// <seealso cref="GtmPropertiesMerger.Merge(GtmData, object, bool)"/>
        public static GtmData MapPurchase(OrderInfo order, object additionalData = null, string purpose = null)
        {
            return HelperObject.MapPurchaseInternal(order, additionalData, purpose);
        }


        /// <summary>
        /// Maps <see cref="OrderInfo"/> to <see cref="GtmData"/> representing Google Tag Manager order object.
        /// Order contains only data about order and do not contain order items array.
        /// </summary>
        /// <param name="order"><see cref="OrderInfo"/> to be mapped to Google Tag Manager order object.</param>
        /// <param name="additionalData">Data with additional non-conflicting key value pairs to be merged with <paramref name="order"/>.</param>
        /// <param name="purpose">Contextual information fitting for customizations.</param>
        /// <returns>The <see cref="GtmData"/> that represents Google Tag Manger order object.</returns>
        /// <seealso cref="MapOrderInternal(OrderInfo, object, string)"/>
        /// <seealso cref="GtmPropertiesMerger.Merge(GtmData, object, bool)"/>
        public static GtmData MapOrder(OrderInfo order, object additionalData = null, string purpose = null)
        {
            return HelperObject.MapOrderInternal(order, additionalData, purpose);
        }


        /// <summary>
        /// Maps <see cref="OrderItemInfo"/>s to list of <see cref="GtmData"/> representing Google Tag Manager order item object.
        /// Order items contain products array in given <paramref name="order"/> and does not contain any order data.
        /// </summary>
        /// <param name="order">Items of <see cref="OrderInfo"/> to be mapped to list of Google Tag Manager order item object.</param>
        /// <param name="additionalData">Data with additional non-conflicting key value pairs to be merged with each mapped <see cref="OrderItemInfo"/>.</param>
        /// <param name="purpose">Contextual information fitting for customizations.</param>
        /// <returns>The <see cref="GtmData"/> that represents Google Tag Manger order items object.</returns>
        /// <seealso cref="MapOrderItemsInternal(OrderInfo, object, string)"/>
        /// <seealso cref="GtmPropertiesMerger.Merge(GtmData, object, bool)"/>
        public static IEnumerable<GtmData> MapOrderItems(OrderInfo order, object additionalData = null, string purpose = null)
        {
            return HelperObject.MapOrderItemsInternal(order, additionalData, purpose);
        }


        /// <summary>
        /// Maps <see cref="OrderInfo"/> to <see cref="GtmData"/> representing Google Tag Manager purchase.
        /// Purchase consists of order and its items.
        /// </summary>
        /// <param name="order"><see cref="OrderInfo"/> to be mapped to be mapped to Google Tag Manager purchase object.</param>
        /// <param name="additionalData">Data with additional non-conflicting key value pairs to be merged resulting <see cref="GtmData"/> object.</param>
        /// <param name="purpose">Contextual information fitting for customizations.</param>
        /// <returns>The <see cref="GtmData"/> that represents Google Tag Manger purchase object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="order"/> is null.</exception>
        protected virtual GtmData MapPurchaseInternal(OrderInfo order, object additionalData = null, string purpose = null)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            var mergedData = new GtmData();

            mergedData.Add("actionField", MapOrderInternal(order, null, purpose));
            mergedData.Add("products", MapOrderItemsInternal(order, null, purpose));

            var gtmPurchase = GtmPropertiesMerger.Merge(mergedData, additionalData);

            return gtmPurchase;
        }


        /// <summary>
        /// Maps <see cref="OrderInfo"/> to <see cref="GtmData"/> representing Google Tag Manager order object.
        /// Order contains only data about order and do not contain any order items array.
        /// </summary>
        /// <param name="order"><see cref="OrderInfo"/> to be mapped to Google Tag Manager order object.</param>
        /// <param name="additionalData">Data with additional non-conflicting key value pairs to be merged with <paramref name="order"/>.</param>
        /// <param name="purpose">Contextual information fitting for customizations.</param>
        /// <returns>The <see cref="GtmData"/> that represents Google Tag Manger order object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="order"/> is null.</exception>
        protected virtual GtmData MapOrderInternal(OrderInfo order, object additionalData = null, string purpose = null)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            var mergedData = new GtmData();

            mergedData.Add("id", order.OrderID);
            mergedData.Add("revenue", order.OrderGrandTotal);
            mergedData.Add("tax", order.OrderTotalTax);
            mergedData.Add("shipping", order.OrderTotalShipping);

            var gtmData = GtmPropertiesMerger.Merge(mergedData, additionalData);

            return gtmData;
        }


        /// <summary>
        /// Maps <see cref="OrderItemInfo"/>s to list of <see cref="GtmData"/> representing Google Tag Manager order item objects.
        /// Order items contain products array in given <paramref name="order"/> and does not contain any order data.
        /// </summary>
        /// <param name="order">Items of <see cref="OrderInfo"/> to be mapped to list of Google Tag Manager order item object.</param>
        /// <param name="additionalData">Data with additional non-conflicting key value pairs to be merged with each mapped <see cref="OrderItemInfo"/>.</param>
        /// <param name="purpose">Contextual information fitting for customizations.</param>
        /// <returns>The <see cref="GtmData"/> that represents Google Tag Manger order items object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="order"/> is null.</exception>
        protected virtual IEnumerable<GtmData> MapOrderItemsInternal(OrderInfo order, object additionalData = null, string purpose = null)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            foreach (var orderItemData in GetOrderItems(order))
            {
                var orderData = new GtmData();

                orderData.Add("name", orderItemData.OrderItem.OrderItemSKUName);
                orderData.Add("id", orderItemData.SKU.SKUNumber);
                orderData.Add("price", orderItemData.OrderItem.OrderItemUnitPrice);

                int skuBrandId = 0;
                if (orderItemData.Brand != null)
                {
                    orderData.Add("brand", orderItemData.Brand.BrandDisplayName);
                }
                // SKUBrandID fallbacks to its parent
                else if ((skuBrandId = orderItemData.SKU.SKUBrandID) > 0)
                {
                    orderData.Add("brand", BrandInfoProvider.GetBrandInfo(skuBrandId).BrandDisplayName);
                }

                orderData.Add("quantity", orderItemData.OrderItem.OrderItemUnitCount);

                var gtmOrderItem = GtmPropertiesMerger.Merge(orderData, additionalData);

                yield return gtmOrderItem;
            }
        }


        /// <summary>
        /// Returns all <see cref="OrderItemInfoData"/> related to the <paramref name="order"/>.
        /// </summary>
        /// <param name="order">Order from which to retrieve all order items.</param>
        internal virtual IEnumerable<OrderItemInfoData> GetOrderItems(OrderInfo order)
        {
            var orderItems = OrderItemInfoProvider.GetOrderItems().WhereEquals(nameof(OrderItemInfo.OrderItemOrderID), order.OrderID)
                .Source(s => s.InnerJoin<SKUInfo>("COM_OrderItem." + nameof(OrderItemInfo.OrderItemSKUID), nameof(SKUInfo.SKUID))
                              .LeftJoin<BrandInfo>("COM_SKU." + nameof(SKUInfo.SKUBrandID), nameof(BrandInfo.BrandID))).Result;


            foreach (DataRow row in orderItems.Tables[0].Rows)
            {
                var brand = new BrandInfo(row);
                var sku = new SKUInfo(row);
                var orderItem = new OrderItemInfo(row);

                yield return new OrderItemInfoData
                {
                    OrderItem = orderItem,
                    SKU = sku,
                    Brand = brand.BrandID > 0 ? brand : null
                };
            }
        }
    }
}
