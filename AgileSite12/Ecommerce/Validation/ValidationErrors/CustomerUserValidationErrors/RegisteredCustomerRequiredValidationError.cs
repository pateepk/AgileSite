namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a validation error resulting from missing a customer where necessary.
    /// </summary>
    /// <seealso cref="ShoppingCartItemValidator"/>
    public class RegisteredCustomerRequiredValidationError : ShoppingCartItemValidationError
    {
        /// <summary>
        /// Creates instance of <see cref="RegisteredCustomerRequiredValidationError"/>.
        /// </summary>
        /// <param name="skuId">SKU Id of the product for which validation failed.</param>
        /// <param name="skuName">SKU name of the product for which validation failed.</param>
        public RegisteredCustomerRequiredValidationError(int skuId, string skuName)
            : base(skuId, skuName, "com.membershipforanonym")
        {

        }
    }
}
