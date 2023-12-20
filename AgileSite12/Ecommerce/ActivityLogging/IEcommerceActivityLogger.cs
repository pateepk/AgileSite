using CMS.Activities;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides possibility to log E-commerce activities.
    /// </summary>
    public interface IEcommerceActivityLogger
    {
        /// <summary>
        /// Logs activity product added to shopping cart.
        /// </summary>
        /// <param name="sku">SKU info object.</param>
        /// <param name="quantity">SKU quantity.</param>
        void LogProductAddedToShoppingCartActivity(SKUInfo sku, int quantity);


        /// <summary>
        /// Logs activity product removed from shopping cart.
        /// </summary>
        /// <param name="sku">SKU info object.</param>
        /// <param name="quantity">SKU quantity.</param>
        /// <param name="contactID">If set overrides <see cref="ActivityInfo.ActivityContactID" /> of activity.</param>
        void LogProductRemovedFromShoppingCartActivity(SKUInfo sku, int quantity, int? contactID = null);


        /// <summary>
        /// Logs activity product added to wish list.
        /// </summary>
        /// <param name="sku">SKUInfo object.</param>
        void LogProductAddedToWishlistActivity(SKUInfo sku);


        /// <summary>
        /// Logs purchase activity.
        /// </summary>
        /// <param name="orderID">Order ID.</param>
        /// <param name="totalPrice">Order total price in main currency.</param>
        /// <param name="totalPriceInCorrectCurrency">String representation of order total price in main currency.</param>
        /// <param name="loggingDisabledInAdministration"><c>True</c> if activities should not be logged in administration.</param>
        /// <param name="contactID">If set overrides <see cref="ActivityInfo.ActivityContactID" /> of activity.</param>
        void LogPurchaseActivity(int orderID, decimal totalPrice, string totalPriceInCorrectCurrency, bool loggingDisabledInAdministration, int? contactID = null);


        /// <summary>
        /// Logs product purchased activity.
        /// </summary>
        /// <param name="sku">SKUInfo object.</param>
        /// <param name="skuUnits">SKU units.</param>
        /// <param name="contactID">If set overrides <see cref="ActivityInfo.ActivityContactID" /> of activity.</param>
        void LogPurchasedProductActivity(SKUInfo sku, int skuUnits, int? contactID = null);
    }
}
