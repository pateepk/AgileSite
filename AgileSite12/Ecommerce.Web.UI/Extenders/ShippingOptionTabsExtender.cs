using System;
using System.Collections.Generic;

using CMS;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.Membership;
using CMS.Modules;
using CMS.UIControls;

[assembly: RegisterCustomClass("ShippingOptionTabsExtender", typeof(CMS.Ecommerce.Web.UI.ShippingOptionTabsExtender))]

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Extender for UITabs control used in detail of shipping option. Adds extra configuration tab if shipping carrier providers any configuration.
    /// </summary>
    internal sealed class ShippingOptionTabsExtender : UITabsExtender
    {
        private ShippingOptionInfo editedOption;

        /// <summary>
        /// Initialization of tabs.
        /// </summary>
        public override void OnInitTabs()
        {
            var optionId = QueryHelper.GetInteger("objectid", 0);
            editedOption = ShippingOptionInfoProvider.GetShippingOptionInfo(optionId);

            Control.OnTabsLoaded += Control_OnTabsLoaded;
        }


        private void Control_OnTabsLoaded(List<UITabItem> tabs)
        {
            // Do not add configuration tab if no option present
            if (editedOption == null)
            {
                return;
            }

            // Check if carrier for shipping option exists
            var carrier = CarrierInfoProvider.GetCarrierProvider(editedOption.ShippingOptionCarrierID);
            if (carrier == null)
            {
                return;
            }

            // Check if carrier provider provides additional service configuration
            var configGuid = carrier.GetServiceConfigurationUIElementGUID(editedOption.ShippingOptionCarrierServiceName);
            if (configGuid == Guid.Empty)
            {
                return;
            }

            // Get UI element and add tab for it
            var configElement = UIElementInfoProvider.GetUIElementInfo(configGuid);
            if ((configElement != null) && UserIsAuthorizedForUIElement(configElement))
            {
                AddTab(tabs, configElement);
            }
        }


        /// <summary>
        /// Adds tab created from UI element. If tab with the same name already exists, nothing is added.
        /// </summary>
        /// <param name="tabs">List of tabs to add tab to.</param>
        /// <param name="element">UI element to be add tab for.S</param>
        private void AddTab(List<UITabItem> tabs, UIElementInfo element)
        {
            if (!tabs.Exists(tab => element.ElementName.EqualsCSafe(tab.TabName, true)))
            {
                tabs.Add(CreateTab(element, tabs.Count));
            }
        }

        
        /// <summary>
        /// Checks if current user can see given UI element.
        /// </summary>
        /// <param name="element">UI Element to check.</param>
        /// <returns>True when authorized.</returns>
        private bool UserIsAuthorizedForUIElement(UIElementInfo element)
        {
            var resource = ResourceInfoProvider.GetResourceInfo(element.ElementResourceID);
            return (resource != null) && MembershipContext.AuthenticatedUser.IsAuthorizedPerUIElement(resource.ResourceName, element.ElementName);
        }


        /// <summary>
        /// Creates the tab
        /// </summary>
        /// <param name="uiElem">UI Element</param>
        /// <param name="tabIndex">Tab index</param>
        private UITabItem CreateTab(UIElementInfo uiElem, int tabIndex)
        {
            var caption = UIElementInfoProvider.GetElementCaption(uiElem);

            // Create element URL
            var url = UIContextHelper.GetElementUrl(uiElem, Control.UIContext);
            url = Control.HandleTabQueryString(url, uiElem);
            url = UrlResolver.ResolveUrl(url);

            var tab = new UITabItem
            {
                Index = tabIndex,
                TabName = uiElem.ElementName,
                Text = HTMLHelper.HTMLEncode(caption),
                RedirectUrl = url,
            };

            return tab;
        }
    }
}