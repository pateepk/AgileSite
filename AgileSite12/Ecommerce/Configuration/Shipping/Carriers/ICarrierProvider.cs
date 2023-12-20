using System;
using System.Collections.Generic;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface providing base methods and properties for carrier providers.
    /// </summary>
    public interface ICarrierProvider : IShippingPriceCalculator
    {
        /// <summary>
        /// Carrier provider name.
        /// </summary>
        string CarrierProviderName
        {
            get;
        }


        /// <summary>
        /// Returns list of service names and their display names.
        /// </summary>
        List<KeyValuePair<string, string>> GetServices();


        /// <summary>
        /// Carrier can deliver given delivery.
        /// </summary>
        /// <param name="delivery">Delivery to be checked.</param>
        bool CanDeliver(Delivery delivery);


        /// <summary>
        /// Returns Guid of UIElement that is used to configure carrier provider.
        /// Return Guid.Empty if carrier does not have configuration UI.
        /// </summary>
        Guid GetConfigurationUIElementGUID();


        /// <summary>
        /// Returns Guid of UIElement that is used to configure service properties.
        /// Return Guid.Empty if service does not have configuration UI.
        /// </summary>
        /// <param name="serviceName">Service name</param>
        Guid GetServiceConfigurationUIElementGUID(string serviceName);
    }
}
