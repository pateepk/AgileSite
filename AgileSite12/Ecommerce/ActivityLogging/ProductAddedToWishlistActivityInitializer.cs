using CMS.Activities;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Initializes activity product added to wishlist. 
    /// </summary>
    internal class ProductAddedToWishlistActivityInitializer : IActivityInitializer
    {
        private readonly int mSkuID;
        private readonly string mSkuName;
        private readonly int mVariantID;
        private readonly ActivityTitleBuilder mTitleBuilder = new ActivityTitleBuilder();


        /// <summary>
        /// Initializes new instance of <see cref="ProductAddedToWishlistActivityInitializer"/>.
        /// </summary>
        /// <param name="skuID">Sku id</param>
        /// <param name="skuName">Sku name</param>
        /// <param name="variantID">SKU id of the selected variant (zero for products without variants)</param>
        public ProductAddedToWishlistActivityInitializer(int skuID, string skuName, int variantID = 0)
        {
            mSkuID = skuID;
            mSkuName = skuName;
            mVariantID = variantID;
        }


        /// <summary>
        /// Adds information about quantity, product id and name to <see cref="IActivityInfo"/>.
        /// </summary>
        /// <param name="activity"></param>
        public void Initialize(IActivityInfo activity)
        {
            activity.ActivityItemID = mSkuID;
            activity.ActivityItemDetailID = mVariantID;
            activity.ActivityTitle = mTitleBuilder.CreateTitle(ActivityType, mSkuName);
        }
        

        /// <summary>
        /// Activity type.
        /// </summary>
        public string ActivityType
        {
            get
            {
                return PredefinedActivityType.PRODUCT_ADDED_TO_WISHLIST;
            }
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMAddingProductToWL";
            }
        }
    }
}
