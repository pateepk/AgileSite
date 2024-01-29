using System;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Default implementation of <see cref="IShippingPriceCalculatorFactory"/>.
    /// </summary>
    public class ShippingPriceCalculatorFactory : IShippingPriceCalculatorFactory
    {
        /// <summary>
        /// Returns a calculator of shipping costs for specified <paramref name="shippingOption"/>.
        /// </summary>
        /// <param name="shippingOption">Shipping option to get calculator for.</param>
        public IShippingPriceCalculator GetCalculator(ShippingOptionInfo shippingOption)
        {
            if (shippingOption == null)
            {
                throw new ArgumentNullException(nameof(shippingOption));
            }

            return CarrierInfoProvider.GetCarrierProvider(shippingOption.ShippingOptionCarrierID);
        }
    }
}