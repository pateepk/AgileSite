using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class representing one item in <see cref="Delivery"/>.
    /// </summary>
    /// <seealso cref="Delivery"/>
    public sealed class DeliveryItem
    {
        /// <summary>
        /// The quantity of the item.
        /// </summary>
        public decimal Amount
        {
            get;
            internal set;
        }


        /// <summary>
        /// The product represented by the item.
        /// </summary>
        public SKUInfo Product
        {
            get;
            internal set;
        }


        /// <summary>
        /// Custom data of this item.
        /// </summary>
        public IDataContainer CustomData
        {
            get;
            internal set;
        }


        /// <summary>
        /// Creates new delivery item using data from given cart item.
        /// </summary>
        /// <param name="cartItem">Cart item to init delivery item with.</param>
        internal DeliveryItem(ShoppingCartItemInfo cartItem)
        {
            Amount = cartItem.CartItemUnits;
            Product = cartItem.SKU;
        }


        /// <summary>
        /// Creates new delivery item using data from given calculation request item.
        /// </summary>
        /// <param name="requestItem">Request item to init delivery item with.</param>
        internal DeliveryItem(CalculationRequestItem requestItem)
        {
            Amount = requestItem.Quantity;
            Product = requestItem.SKU;
        }
    }
}
