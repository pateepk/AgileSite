namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a validation error resulting from store not having enough units of product in stock.
    /// </summary>
    /// <seealso cref="ShoppingCartItemValidator"/>
    public class SKUNotInStockValidationError : ShoppingCartItemValidationError
    {
        /// <summary>
        /// Number of units available in stock.
        /// </summary>
        public int InventoryUnits
        {
            get;
        }


        /// <summary>
        /// Returns an array containing SKU name and units in stock for the SKU.
        /// </summary>
        public override object[] MessageParameters => new object[] { SKUName, InventoryUnits };


        /// <summary>
        /// Creates instance of <see cref="SKUNotInStockValidationError"/>.
        /// </summary>
        /// <param name="skuId">SKU Id of the product for which validation failed.</param>
        /// <param name="skuName">SKU name of the product for which validation failed.</param>
        /// <param name="inventoryUnits">Number of units available in stock.</param>
        public SKUNotInStockValidationError(int skuId, string skuName, int inventoryUnits)
            : base(skuId, skuName, "com.inventory.notenoughunits")
        {
            InventoryUnits = inventoryUnits;
        }
    }
}
