namespace CMS.Ecommerce
{
    /// <summary>
    /// Defines the contract for classes responsible for the tax address resolution.
    /// </summary>
    public interface ITaxAddressService
    {
        /// <summary>
        /// Returns an address using which the tax should be calculated.
        /// </summary>
        /// <remarks>
        /// Note that the result can be <paramref name="billingAddress"/>, <paramref name="shippingAddress"/> or any other <see cref="AddressInfo"/> instance.
        /// </remarks>
        /// <param name="billingAddress">A billing address of taxed purchase.</param>
        /// <param name="shippingAddress">A address where the goods are delivered.</param>
        /// <param name="taxClass">A tax class to get address for.</param>
        /// <param name="customer">A customer making a taxed purchase.</param>
        AddressInfo GetTaxAddress(AddressInfo billingAddress, AddressInfo shippingAddress, TaxClassInfo taxClass, CustomerInfo customer);
    }
}
