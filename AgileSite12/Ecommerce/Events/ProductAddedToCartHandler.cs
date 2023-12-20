using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Product added to shopping cart event handler.
    /// </summary>
    public sealed class ProductAddedToCartHandler : SimpleHandler<ProductAddedToCartHandler, ProductAddedToCartEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="addedShoppingCartItem">Product added to shopping cart</param>
        public ProductAddedToCartEventArgs StartEvent(ShoppingCartItemInfo addedShoppingCartItem)
        {
            var args = new ProductAddedToCartEventArgs(addedShoppingCartItem);

            StartEvent(args);

            return args;
        }
    }
}
