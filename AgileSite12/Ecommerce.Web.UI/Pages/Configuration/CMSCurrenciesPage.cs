using System;

using CMS.Core;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Base page for the E-commerce currencies to apply global settings to the pages.
    /// </summary>
    public class CMSCurrenciesPage : CMSEcommerceConfigurationPage
    {
        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Check UI element
            var elementName = IsMultiStoreConfiguration ? "Tools.Ecommerce.Currencies" : "Configuration.Currencies";
            CheckUIElementAccessHierarchical(ModuleName.ECOMMERCE, elementName);

            // Set object type for allowing global currencies
            SiteOrGlobalObjectType = CurrencyInfo.OBJECT_TYPE;
        }
    }
}