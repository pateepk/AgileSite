namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a validation error resulting from an excess of product units in cart.
    /// </summary>
    /// <seealso cref="ShoppingCartItemValidator"/>
    public class SKUMaxUnitsExceededValidationError : ShoppingCartItemValidationError
    {
        /// <summary>
        /// Maximum allowed units for the product.
        /// </summary>
        public int MaxUnits
        {
            get;
        }


        /// <summary>
        /// Returns an array containing SKU name and maximum units for the SKU.
        /// </summary>
        public override object[] MessageParameters => new object[] { SKUName, MaxUnits };


        /// <summary>
        /// Creates instance of <see cref="SKUMaxUnitsExceededValidationError"/>.
        /// </summary>
        /// <param name="skuId">SKU Id of the product for which validation failed.</param>
        /// <param name="skuName">SKU name of the product for which validation failed.</param>
        /// <param name="maxUnits">Maximum number of units for the product.</param>
        public SKUMaxUnitsExceededValidationError(int skuId, string skuName, int maxUnits)
            : base(skuId, skuName, "com.inventory.maxunitsexceeded")
        {
            MaxUnits = maxUnits;
        }
    }
}
