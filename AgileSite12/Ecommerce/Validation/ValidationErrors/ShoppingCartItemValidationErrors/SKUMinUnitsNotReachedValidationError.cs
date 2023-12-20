namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a validation error resulting from not having enough of product units in cart.
    /// </summary>
    /// <seealso cref="ShoppingCartItemValidator"/>
    public class SKUMinUnitsNotReachedValidationError : ShoppingCartItemValidationError
    {
        /// <summary>
        /// Minimum amount of units to have in cart.
        /// </summary>
        public int MinUnits
        {
            get;
        }


        /// <summary>
        /// Returns an array containing SKU name and minimum units for the SKU.
        /// </summary>
        public override object[] MessageParameters => new object[] { SKUName, MinUnits };


        /// <summary>
        /// Creates instance of <see cref="SKUMinUnitsNotReachedValidationError"/>.
        /// </summary>
        /// <param name="skuId">SKU Id of the product for which validation failed.</param>
        /// <param name="skuName">SKU name of the product for which validation failed.</param>
        /// <param name="minUnits">Minimum number of units for the product.</param>
        public SKUMinUnitsNotReachedValidationError(int skuId, string skuName, int minUnits)
            : base(skuId, skuName, "com.inventory.minunitsnotreached")
        {
            MinUnits = minUnits;
        }
    }
}
