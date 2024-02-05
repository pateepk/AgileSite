using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.UIControls;

[assembly: RegisterCustomClass("CarriersListExtender", typeof(CMS.Ecommerce.Web.UI.CarriersListExtender))]

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Carrier list extender.
    /// </summary>
    internal sealed class CarriersListExtender : ControlExtender<UniGrid>
    {
        public override void OnInit()
        {
            if (Control != null)
            {
                Control.OnExternalDataBound += OnExternalDataBound;
                Control.OnAction += UnigridOnAction;
            }
        }


        /// <summary>
        /// Handles the UniGrid's OnAction event.
        /// </summary>
        private void UnigridOnAction(string actionName, object actionArgument)
        {
            if (actionName.EqualsCSafe("delete"))
            {
                int carrierID = ValidationHelper.GetInteger(actionArgument, 0);
                var carrierShippingOptions = ShippingOptionInfoProvider.GetShippingOptions(SiteContext.CurrentSiteID).WhereEquals("ShippingOptionCarrierID", carrierID);

                List<string> dependencies = new List<string>();

                // Check if shipping options have dependencies
                foreach (var option in carrierShippingOptions.Where(option => option.Generalized.CheckDependencies(false)))
                {
                    dependencies.AddRange(option.Generalized.GetDependenciesNames(topN: 5));
                }

                if (dependencies.Count > 0)
                {
                    Control.ShowError(EcommerceUIHelper.FormatDependencyMessage(dependencies, ResHelper.GetString("Ecommerce.DeleteCarrier"), "</br>"));
                    return;
                }

                // Delete carrier
                CarrierInfoProvider.DeleteCarrierInfo(carrierID);
            }
        }


        /// <summary>
        /// Displays user friendly carrier provider name in listing.
        /// </summary>
        private object OnExternalDataBound(object sender, string sourceName, object parameter)
        {
            if (sourceName.ToLowerCSafe() == "carrierprovider")
            {
                int carrierID = ValidationHelper.GetInteger(parameter, 0);
                var carrierProvider = CarrierInfoProvider.GetCarrierProvider(carrierID);

                if (carrierProvider != null)
                {
                    return HTMLHelper.HTMLEncode(ResHelper.LocalizeString(carrierProvider.CarrierProviderName));
                }
            }

            return parameter;
        }
    }
}