using CMS.Activities;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Initializes activity purchased product.
    /// </summary>
    internal class PurchasedProductActivityInitializer : IActivityInitializer
    {
        private readonly ActivityTitleBuilder mTitleBuilder = new ActivityTitleBuilder();
        private readonly int mSkuID;
        private readonly int mSkuUnits;
        private readonly int mVariantID;
        private readonly string mSkuName;


        public PurchasedProductActivityInitializer(int skuID, string skuName, int skuUnits, int variantID = 0)
        {
            mSkuID = skuID;
            mVariantID = variantID;
            mSkuUnits = skuUnits;
            mSkuName = skuName;
        }


        public void Initialize(IActivityInfo activity)
        {
            activity.ActivityItemID = mSkuID;
            activity.ActivityItemDetailID = mVariantID;
            activity.ActivityValue = mSkuUnits.ToString();
            activity.ActivityTitle = mTitleBuilder.CreateTitle(ActivityType, mSkuName);
        }


        public string ActivityType
        {
            get
            {
                return PredefinedActivityType.PURCHASEDPRODUCT;
            }
        }


        public string SettingsKeyName
        {
            get
            {
                return "CMSCMPurchasedProduct";
            }
        }
    }
}