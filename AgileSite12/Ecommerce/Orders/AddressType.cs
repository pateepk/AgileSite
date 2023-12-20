namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents the type of address.
    /// </summary>
    public enum AddressType
    {
        /// <summary>
        /// Unknown address
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Billing address
        /// </summary>
        Billing = 1,

        /// <summary>
        /// Shipping address
        /// </summary>
        Shipping = 2,

        /// <summary>
        /// Company address
        /// </summary>
        Company = 3
    }
}