using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IShippingPriceCalculatorFactory), typeof(ShippingPriceCalculatorFactory), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Contract for factories creating instances of <see cref="IShippingPriceCalculator"/>.
    /// </summary>
    public interface IShippingPriceCalculatorFactory
    {
        /// <summary>
        /// Returns a calculator of shipping costs for specified <paramref name="shippingOption"/>.
        /// </summary>
        /// <param name="shippingOption">Shipping option to get calculator for.</param>
        IShippingPriceCalculator GetCalculator(ShippingOptionInfo shippingOption);
    }
}