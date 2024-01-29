using CMS.Core;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Base page for the E-commerce pubic statuses to apply global settings to the pages.
    /// </summary>
    public class CMSPublicStatusesPage : CMSEcommerceConfigurationPage
    {
        /// <summary>
        /// Creates child controls and sets global objects key name.
        /// </summary>
        protected override void CreateChildControls()
        {
            // Set object type for allowing global public statuses
            SiteOrGlobalObjectType = PublicStatusInfo.OBJECT_TYPE;

            // Check UI element
            var elementName = IsMultiStoreConfiguration ? "Tools.Ecommerce.PublicStatus" : "Configuration.PublicStatus";
            CheckUIElementAccessHierarchical(ModuleName.ECOMMERCE, elementName);

            base.CreateChildControls();
        }
    }
}