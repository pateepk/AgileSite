using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(ICustomerTaxClassService), typeof(CustomerTaxClassService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Defines the contract for classes responsible for the categorization of customers into customer tax classes.
    /// </summary>
    public interface ICustomerTaxClassService
    {
        /// <summary>
        /// Returns a <see cref="CustomerTaxClass"/> for the specified customer. The default value is <see cref="CustomerTaxClass.Taxable"/>
        /// </summary>
        /// <param name="customer">A customer to be classified.</param>
        CustomerTaxClass GetTaxClass(CustomerInfo customer);
    }
}
