using CMS.Core;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Base page for the E-commerce order statuses to apply global settings to the pages.
    /// </summary>
    public class CMSOrderStatusesPage : CMSEcommerceConfigurationPage
    {
        /// <summary>
        /// Creates child controls and sets global objects key name.
        /// </summary>
        protected override void CreateChildControls()
        {
            // Set object type for allowing global order statuses
            SiteOrGlobalObjectType = OrderStatusInfo.OBJECT_TYPE;

            // Check UI element
            var elementName = IsMultiStoreConfiguration ? "Tools.Ecommerce.OrderStatus" : "Configuration.OrderStatus";
            CheckUIElementAccessHierarchical(ModuleName.ECOMMERCE, elementName);

            base.CreateChildControls();
        }
    }
}