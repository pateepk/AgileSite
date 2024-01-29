using CMS.Activities;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Initializes activity product added to shopping cart
    /// </summary>
    internal class ProductAddedToShoppingCartActivityInitializer : IActivityInitializer
    {
        private readonly int mQuantity;
        private readonly int mSkuID;
        private readonly int mVariantID;
        private readonly string mSkuName;
        private readonly ActivityTitleBuilder mTitleBuilder = new ActivityTitleBuilder();


        /// <summary>
        /// Initializes new instance of <see cref="ProductAddedToShoppingCartActivityInitializer"/>.
        /// </summary>
        /// <param name="quantity">SKU quantity</param>
        /// <param name="skuId">SKU id</param>
        /// <param name="skuName">SKU name</param>
        /// <param name="variantID">SKU ID of the selected variant (zero for products without variants)</param>
        public ProductAddedToShoppingCartActivityInitializer(int quantity, int skuId, string skuName, int variantID = 0)
        {
            mQuantity = quantity;
            mSkuID = skuId;
            mVariantID = variantID;
            mSkuName = skuName;
        }


        /// <summary>
        /// Adds information about quantity, product id and name to <see cref="IActivityInfo"/>.
        /// </summary>
        /// <param name="activity"></param>
        public void Initialize(IActivityInfo activity)
        {
            activity.ActivityValue = mQuantity.ToString();
            activity.ActivityItemID = mSkuID;
            activity.ActivityItemDetailID = mVariantID;
            activity.ActivityTitle = mTitleBuilder.CreateTitle(ActivityType, mSkuName);
        }


        public string ActivityType
        {
            get
            {
                return PredefinedActivityType.PRODUCT_ADDED_TO_SHOPPINGCART;
            }
        }


        public string SettingsKeyName
        {
            get
            {
                return "CMSCMAddingProductToSC";
            }
        }
    }
}