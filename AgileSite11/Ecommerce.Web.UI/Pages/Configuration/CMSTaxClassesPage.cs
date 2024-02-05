namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Base page for the E-commerce tax classes to apply global settings to the pages.
    /// </summary>
    public class CMSTaxClassesPage : CMSEcommerceConfigurationPage
    {
        /// <summary>
        /// Creates child controls and sets global objects key name.
        /// </summary>
        protected override void CreateChildControls()
        {
            // Set object type for allowing global tax classes
            SiteOrGlobalObjectType = TaxClassInfo.OBJECT_TYPE;

            base.CreateChildControls();
        }
    }
}