using System.Web;

using CMS.Activities;
using CMS.Core;
using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides possibility to log E-commerce activities.
    /// </summary>
    /// <remarks>
    /// Parent product information is logged for product variants. 
    /// VariantID is logged into <see cref="IActivityInfo.ActivityItemDetailID"/> field in case that given <see cref="SKUInfo"/> object is product variant.
    /// </remarks>
    public class EcommerceActivityLogger
    {
        /// <summary>
        /// Activity log service instance
        /// </summary>
        protected readonly IActivityLogService mActivityLogService = Service.Resolve<IActivityLogService>();


        /// <summary>
        /// Logs activity product added to shopping cart.
        /// </summary>
        /// <param name="sku">SKU info object.</param>
        /// <param name="quantity">SKU quantity</param>
        public void LogProductAddedToShoppingCartActivity(SKUInfo sku, int quantity)
        {
            var activityInitializer = new ProductAddedToShoppingCartActivityInitializer(quantity, GetSKUID(sku), GetSKUName(sku), GetVariantID(sku));
            mActivityLogService.Log(activityInitializer, GetCurrentRequest());
        }


        /// <summary>
        /// Logs activity product removed from shopping cart.
        /// </summary>
        /// <param name="sku">SKU info object.</param>
        /// <param name="quantity">SKU quantity</param>
        /// <param name="contactID">If set overrides <see cref="ActivityInfo.ActivityContactID" /> of activity</param>
        public void LogProductRemovedFromShoppingCartActivity(SKUInfo sku, int quantity, int? contactID = null)
        {
            IActivityInitializer activityInitializer = new ProductRemovedFromShoppingCartActivityInitializer(quantity, GetSKUID(sku), GetSKUName(sku), GetVariantID(sku));
            activityInitializer = contactID.HasValue ? activityInitializer.WithContactId(contactID.Value) : activityInitializer;
            mActivityLogService.Log(activityInitializer, GetCurrentRequest());
        }


        /// <summary>
        /// Logs activity product added to wish list.
        /// </summary>
        /// <param name="sku">SKUInfo object.</param>
        public void LogProductAddedToWishlistActivity(SKUInfo sku)
        {
            var activityInitializer = new ProductAddedToWishlistActivityInitializer(GetSKUID(sku), GetSKUName(sku), GetVariantID(sku));
            mActivityLogService.Log(activityInitializer, GetCurrentRequest());
        }


        /// <summary>
        /// Logs purchase activity.
        /// </summary>
        /// <param name="orderID">Order ID</param>
        /// <param name="totalPrice">Order total price in main currency</param>
        /// <param name="totalPriceInCorrectCurrency">String representation of order total price in main currency</param>
        /// <param name="loggingDisabledInAdministration"><c>True</c> if activities should not be logged in administration</param>
        /// <param name="contactID">If set overrides <see cref="ActivityInfo.ActivityContactID" /> of activity</param>
        public void LogPurchaseActivity(int orderID, decimal totalPrice, string totalPriceInCorrectCurrency, bool loggingDisabledInAdministration, int? contactID = null)
        {
            IActivityInitializer activityInitializer = new PurchaseActivityInitializer(orderID, totalPrice, totalPriceInCorrectCurrency);
            activityInitializer = contactID.HasValue ? activityInitializer.WithContactId(contactID.Value) : activityInitializer;
            mActivityLogService.Log(activityInitializer, GetCurrentRequest(), loggingDisabledInAdministration);
        }


        /// <summary>
        /// Logs product purchased activity.
        /// </summary>
        /// <param name="sku">SKUInfo object.</param>
        /// <param name="skuUnits">SKU units</param>
        /// <param name="contactID">If set overrides <see cref="ActivityInfo.ActivityContactID" /> of activity</param>
        public void LogPurchasedProductActivity(SKUInfo sku, int skuUnits, int? contactID = null)
        {
            IActivityInitializer activityInitializer = new PurchasedProductActivityInitializer(GetSKUID(sku), GetSKUName(sku), skuUnits, GetVariantID(sku));
            activityInitializer = contactID.HasValue ? activityInitializer.WithContactId(contactID.Value) : activityInitializer;
            mActivityLogService.Log(activityInitializer, GetCurrentRequest(), false);
        }


        /// <summary>
        /// Returns current request.
        /// </summary>
        /// <returns>Current request.</returns>
        protected virtual HttpRequestBase GetCurrentRequest()
        {
            return CMSHttpContext.Current.Request;
        }


        private int GetSKUID(SKUInfo sku)
        {
            if (sku.IsProductVariant)
            {
                return sku.SKUParentSKUID;
            }

            return sku.SKUID;
        }


        private int GetVariantID(SKUInfo sku)
        {
            if (sku.IsProductVariant)
            {
                return sku.SKUID;
            }

            return 0;
        }


        private string GetSKUName(SKUInfo sku)
        {
            if (sku.IsProductVariant)
            {
                return ResHelper.LocalizeString(sku.Parent.Generalized.ObjectDisplayName);
            }

            return ResHelper.LocalizeString(sku.SKUName);
        }
    }
}