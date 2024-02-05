using CMS.Activities;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Initializes abandoned shopping cart activity. 
    /// </summary>
    internal class AbandonedShoppingCartActivityInitializer : IActivityInitializer
    {
        private readonly int mSiteId;
        private readonly ShoppingCartInfo mCart;


        /// <summary>
        /// Initializes new instance of the <see cref="AbandonedShoppingCartActivityInitializer"/>.
        /// </summary>
        /// <param name="siteId">Site id</param>
        /// <param name="cart">Shopping cart</param>
        public AbandonedShoppingCartActivityInitializer(int siteId, ShoppingCartInfo cart)
        {
            mSiteId = siteId;
            mCart = cart;
        }


        /// <summary>
        /// Adds information about active and original contact id, adds activity value as <see cref="ShoppingCartInfo.ShoppingCartGUID"/>, site id and title.
        /// </summary>
        /// <param name="activity">Activity info</param>
        public void Initialize(IActivityInfo activity)
        {
            activity.ActivityTitle = "Shopping cart abandoned";
            activity.ActivitySiteID = mSiteId;
            activity.ActivityContactID = mCart.ShoppingCartContactID; 
            // Shopping cart GUID
            activity.ActivityValue = mCart.ShoppingCartGUID.ToString();
        }


        /// <summary>
        /// Activity type.
        /// </summary>
        public string ActivityType
        {
            get
            {
                return "ShoppingCartAbandoned";
            }
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "";
            }
        }
    }
}