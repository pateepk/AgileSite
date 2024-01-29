namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Base page for the E-commerce payment methods to apply global settings to the pages.
    /// </summary>
    public class CMSPaymentMethodsPage : CMSEcommerceConfigurationPage
    {
        /// <summary>
        /// Creates child controls and sets global objects key name.
        /// </summary>
        protected override void CreateChildControls()
        {
            // Set object type for allowing global payment methods
            SiteOrGlobalObjectType = PaymentOptionInfo.OBJECT_TYPE;

            base.CreateChildControls();
        }
    }
}