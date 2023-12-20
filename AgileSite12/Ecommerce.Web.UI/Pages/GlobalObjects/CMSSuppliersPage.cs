using System;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Base page for the E-commerce suppliers pages to apply global settings to the pages.
    /// </summary>
    public class CMSSuppliersPage : CMSEcommerceObjectsPage
    {
        /// <summary>
        /// Creates child controls and sets global objects key name.
        /// </summary>
        protected override void CreateChildControls()
        {
            // Set key name for allowing global suppliers
            GlobalObjectsKeyName = ECommerceSettings.ALLOW_GLOBAL_SUPPLIERS;

            base.CreateChildControls();
        }
    }
}