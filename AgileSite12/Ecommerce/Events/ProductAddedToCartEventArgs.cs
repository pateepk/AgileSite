using System;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Event arguments for "product added to shopping cart" event
    /// </summary>
    public sealed class ProductAddedToCartEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Newly added shopping cart item.
        /// </summary>        
        public ShoppingCartItemInfo AddedShoppingCartItem
        {
            get;
        }


        /// <summary>
        /// Initializes new instance of <see cref="ProductAddedToCartEventArgs"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">When null <paramref name="addedShoppingCartItem"/> is provided.</exception>
        public ProductAddedToCartEventArgs(ShoppingCartItemInfo addedShoppingCartItem)
        {
            AddedShoppingCartItem = addedShoppingCartItem ?? throw new ArgumentNullException(nameof(addedShoppingCartItem));
        }
    }
}
