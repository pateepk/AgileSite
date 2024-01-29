using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(ITaxClassService), typeof(TaxClassService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Defines the contract for classes responsible for the product tax class retrieval.
    /// </summary>
    public interface ITaxClassService
    {
        /// <summary>
        /// Gets a tax class to which the shipping option belongs. 
        /// </summary>
        TaxClassInfo GetTaxClass(ShippingOptionInfo shipping);


        /// <summary>
        /// Gets a tax class to which the product belongs.
        /// </summary>
        /// <param name="product">A product, product variant or product accessory.</param>
        /// <returns><see cref="TaxClassInfo"/> for the specified product or <c>null</c> when not found.</returns>
        TaxClassInfo GetTaxClass(SKUInfo product);
    }
}
