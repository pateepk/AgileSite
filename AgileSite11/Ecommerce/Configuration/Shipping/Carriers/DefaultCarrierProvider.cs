using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Default carrier provider encapsulating default shipping cost calculation by weight.
    /// </summary>
    public sealed class DefaultCarrierProvider : ICarrierProvider
    {
        private const string NAME = "{$com.defaultcarrierprovider.name$}";
        private SortedDictionary<string, string> mServices;


        /// <summary>
        /// Gets dictionary of service names and their display names.
        /// </summary>
        private SortedDictionary<string, string> Services
        {
            get
            {
                return mServices ?? (mServices = new SortedDictionary<string, string>
                {
                    {"CostByWeight", "{$com.defaultcarrierprovider.byweightservice$}"}
                });
            }
        }


        /// <summary>
        /// Carrier provider name.
        /// </summary>
        public string CarrierProviderName
        {
            get
            {
                return NAME;
            }
        }


        /// <summary>
        /// Returns list of service names and their display names.
        /// </summary>
        public List<KeyValuePair<string, string>> GetServices()
        {
            return Services.ToList();
        }


        /// <summary>
        /// Calculates shipping costs of delivering given delivery.
        /// </summary>
        /// <param name="delivery">Delivery to be checked.</param>
        /// <param name="currencyCode">Code of currency to return shipping price in (e.g. USD or EUR).</param>
        /// <returns>Price in currency specified by currencyCode.</returns>
        public decimal GetPrice(Delivery delivery, string currencyCode)
        {
            var shippingOption = delivery.ShippingOption;

            // Get shipping cost for chosen shipping option
            var cost = ShippingCostInfoProvider.GetShippingCostInfo(shippingOption.ShippingOptionID, (double)delivery.Weight);
            var bestFitCost = cost?.ShippingCostValue ?? 0m;

            return CurrencyConverter.Convert(bestFitCost, false, currencyCode, shippingOption.ShippingOptionSiteID);
        }


        /// <summary>
        /// Checks if service is available for specified conditions.
        /// </summary>
        /// <param name="delivery">Delivery to be checked.</param>
        public bool CanDeliver(Delivery delivery)
        {
            if (delivery.ShippingOption == null)
            {
                return false;
            }

            return SupportsService(delivery.ShippingOption.ShippingOptionCarrierServiceName);
        }


        /// <summary>
        /// Returns Guid of UIElement that is used to configure carrier provider.
        /// Return Guid.Empty if carrier does not have configuration UI.
        /// </summary>
        public Guid GetConfigurationUIElementGUID()
        {
            return Guid.Empty;
        }


        /// <summary>
        /// Returns Guid of UIElement that is used to configure service properties.
        /// Return Guid.Empty if service does not have configuration UI.
        /// </summary>
        /// <param name="serviceName">Service name</param>
        public Guid GetServiceConfigurationUIElementGUID(string serviceName)
        {
            return new Guid("54A20C4C-8E9D-40BF-BBAE-26927737D3B2");
        }


        /// <summary>
        /// Returns true carrier provider can handle given service.
        /// </summary>
        /// <param name="serviceName">Service name to check.</param>
        private bool SupportsService(string serviceName)
        {
            return Services.ContainsKey(serviceName);
        }
    }
}
