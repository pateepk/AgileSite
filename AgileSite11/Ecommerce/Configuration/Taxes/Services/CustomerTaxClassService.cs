namespace CMS.Ecommerce
{
    /// <summary>
    /// Default implementation of <see cref="ICustomerTaxClassService"/>. 
    /// Handles the classification of customers into customer tax classes according to the <see cref="CustomerInfo.CustomerTaxRegistrationID"/> property.
    /// </summary>
    internal class CustomerTaxClassService : ICustomerTaxClassService
    {
        /// <summary>
        /// Returns <see cref="CustomerTaxClass"/> for the specified customer. The default value is <see cref="CustomerTaxClass.Taxable"/>
        /// </summary>
        /// <param name="customer">A customer to be classified.</param>
        public CustomerTaxClass GetTaxClass(CustomerInfo customer)
        {
            return string.IsNullOrEmpty(customer?.CustomerTaxRegistrationID) ? CustomerTaxClass.Taxable : CustomerTaxClass.Exempt;
        }
    }
}