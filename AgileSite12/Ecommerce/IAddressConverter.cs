namespace CMS.Ecommerce
{
    /// <summary>
    /// Defines methods which converts addresses.
    /// </summary>
    public interface IAddressConverter
    {
        /// <summary>
        /// Converts <see cref="OrderAddressInfo"/> to <see cref="AddressInfo"/>.
        /// </summary>
        /// <param name="orderAddress">Order address to copy data from.</param>
        /// <returns>New <see cref="AddressInfo"/> based on <see cref="OrderAddressInfo"/> data.</returns>
        AddressInfo Convert(OrderAddressInfo orderAddress);

        /// <summary>
        /// Converts <see cref="AddressInfo"/> to <see cref="OrderAddressInfo"/>.
        /// </summary>
        /// <param name="address">Address to copy data from.</param>
        /// <param name="orderId">ID of the order to assign address to.</param>
        /// <param name="addressType">Type of the address.</param>
        /// <returns>New <see cref="OrderAddressInfo"/> based on <see cref="AddressInfo"/> data.</returns>
        OrderAddressInfo Convert(AddressInfo address, int orderId, AddressType addressType);
    }
}
