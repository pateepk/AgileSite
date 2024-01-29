using CMS.Activities;
using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Initializes purchase activity. 
    /// </summary>
    internal class PurchaseActivityInitializer : IActivityInitializer
    {
        private readonly int mOrderId;
        private readonly decimal mTotalPrice;
        private readonly string mTotalPriceInCorrectCurrency;
        private readonly ActivityTitleBuilder mTitleBuilder = new ActivityTitleBuilder();


        /// <summary>
        /// Initializes new instance of <see cref="PurchaseActivityInitializer"/>.
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="totalPrice">Order total price</param>
        /// <param name="totalPriceInCorrectCurrency">String representation of order total price in correct currency</param>
        public PurchaseActivityInitializer(int orderId, decimal totalPrice, string totalPriceInCorrectCurrency)
        {
            mOrderId = orderId;
            mTotalPrice = totalPrice;
            mTotalPriceInCorrectCurrency = totalPriceInCorrectCurrency;
        }


        /// <summary>
        /// Adds information about quantity, product id and name to <see cref="IActivityInfo"/>.
        /// </summary>
        /// <param name="activity">Activity info</param>
        public void Initialize(IActivityInfo activity)
        {
            activity.ActivityItemID = mOrderId;
            activity.ActivityTitle = mTitleBuilder.CreateTitle(ActivityType, mTotalPriceInCorrectCurrency);
            activity.ActivityValue = mTotalPrice.ToString(CultureHelper.EnglishCulture);
        }


        /// <summary>
        /// Activity type.
        /// </summary>
        public string ActivityType
        {
            get
            {
                return PredefinedActivityType.PURCHASE;
            }
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMPurchase";
            }
        }
    }
}
