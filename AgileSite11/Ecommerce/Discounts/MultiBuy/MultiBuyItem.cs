using System;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents an item for Buy X get Y discounts evaluation.
    /// </summary>
    public class MultiBuyItem
    {
        /// <summary>
        /// Gets or sets the ID of the item.
        /// </summary>
        public Guid ID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the SKU behind this item.
        /// </summary>
        public SKUInfo SKU
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the unit price of this item.
        /// </summary>
        public decimal UnitPrice
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the amount of the item.
        /// </summary>
        public decimal Units
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the amount of the automatically added items.
        /// </summary>
        public decimal AutoAddedUnits
        {
            get;
            set;
        }


        /// <summary>
        /// Creates a new instance of <see cref="MultiBuyItem"/> initialized with the specified <paramref name="item"/>.
        /// </summary>
        /// <param name="item">Cart item to create <see cref="MultiBuyItem"/> from.</param>
        internal MultiBuyItem(ShoppingCartItemInfo item)
        {
            ID = item.CartItemGUID;
            SKU = item.SKU;
            UnitPrice = item.UnitPrice;
            Units = item.CartItemUnits;
            AutoAddedUnits = item.CartItemAutoAddedUnits;
        }


        /// <summary>
        /// Creates a new instance of <see cref="MultiBuyItem"/> initialized with the specified <paramref name="item"/>.
        /// </summary>
        /// <param name="item">Calculation request item to create <see cref="MultiBuyItem"/> from.</param>
        /// <param name="unitPrice">Unit price of the product to calculate discount from.</param>
        public MultiBuyItem(CalculationRequestItem item, decimal unitPrice)
        {
            ID = item.ItemGuid;
            SKU = item.SKU;
            UnitPrice = unitPrice;
            Units = item.Quantity;
            AutoAddedUnits = item.AutoAddedQuantity;
        }
    }
}
