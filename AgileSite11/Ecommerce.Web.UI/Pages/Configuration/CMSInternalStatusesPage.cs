using CMS.Core;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Base page for the E-commerce internal statuses to apply global settings to the pages.
    /// </summary>
    public class CMSInternalStatusesPage : CMSEcommerceConfigurationPage
    {
        /// <summary>
        /// Creates child controls and sets global objects key name.
        /// </summary>
        protected override void CreateChildControls()
        {
            // Set object type for allowing global internal statuses
            SiteOrGlobalObjectType = InternalStatusInfo.OBJECT_TYPE;

            // Check UI element
            var elementName = IsMultiStoreConfiguration ? "Tools.Ecommerce.InternalStatus" : "Configuration.InternalStatus";
            CheckUIElementAccessHierarchical(ModuleName.ECOMMERCE, elementName);

            base.CreateChildControls();
        }
    }
}