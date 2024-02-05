using System;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Base page for the E-commerce product options pages to apply global settings to the pages.
    /// </summary>
    public class CMSProductOptionCategoriesPage : CMSEcommerceObjectsPage
    {
        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Set key name for allowing global product options
            GlobalObjectsKeyName = ECommerceSettings.ALLOW_GLOBAL_PRODUCT_OPTIONS;
        }
    }
}