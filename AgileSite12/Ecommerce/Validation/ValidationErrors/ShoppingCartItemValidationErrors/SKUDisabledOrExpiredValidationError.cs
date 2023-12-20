namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a validation error resulting from having a disabled or expired item in cart.
    /// </summary>
    /// <seealso cref="ShoppingCartItemValidator"/>
    public class SKUDisabledOrExpiredValidationError : ShoppingCartItemValidationError
    {
        /// <summary>
        /// Creates instance of <see cref="SKUDisabledOrExpiredValidationError"/>.
        /// </summary>
        /// <param name="skuId">SKU Id of the product for which validation failed.</param>
        /// <param name="skuName">SKU name of the product for which validation failed.</param>
        public SKUDisabledOrExpiredValidationError(int skuId, string skuName)
            :base(skuId, skuName, "com.inventory.productdisabled")
        {

        }
    }
}
