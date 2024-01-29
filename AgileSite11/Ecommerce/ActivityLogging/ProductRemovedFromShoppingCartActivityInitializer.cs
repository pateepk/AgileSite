using CMS.Activities;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Initializes activity product removed from shopping cart
    /// </summary>
    internal class ProductRemovedFromShoppingCartActivityInitializer : IActivityInitializer
    {
        private readonly string mSkuName;
        private readonly int mSkuID;
        private readonly int mVariantID;
        private readonly int mQuantity;
        private readonly ActivityTitleBuilder mTitleBuilder = new ActivityTitleBuilder();


        /// <summary>
        /// Logs activity product removed from shopping cart.
        /// </summary>
        /// <param name="quantity">Sku quantity</param>
        /// <param name="skuID">Sku id</param>
        /// <param name="skuName">Sku name</param>
        /// <param name="variantID">SKU ID of the selected variant (zero for products without variants)</param>
        public ProductRemovedFromShoppingCartActivityInitializer(int quantity, int skuID, string skuName, int variantID = 0)
        {
            mSkuName = skuName;
            mSkuID = skuID;
            mVariantID = variantID;
            mQuantity = quantity;
        }


        /// <summary>
        /// Adds information about quantity, product id and name to <see cref="IActivityInfo"/>.
        /// </summary>
        /// <param name="activity"></param>
        public void Initialize(IActivityInfo activity)
        {
            activity.ActivityTitle = mTitleBuilder.CreateTitle(ActivityType, mSkuName);
            activity.ActivityItemID = mSkuID;
            activity.ActivityItemDetailID = mVariantID;
            activity.ActivityValue = mQuantity.ToString();
        }


        public string ActivityType
        {
            get
            {
                return PredefinedActivityType.PRODUCT_REMOVED_FROM_SHOPPINGCART;
            }
        }


        public string SettingsKeyName
        {
            get
            {
                return "CMSCMRemovingProductFromSC";
            }
        }
    }
}