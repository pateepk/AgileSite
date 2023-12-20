namespace CMS.Ecommerce
{
    /// <summary>
    /// Global e-commerce events
    /// </summary>
    public static class EcommerceEvents
    {
        /// <summary>
        /// Fired when new order was created through checkout process.
        /// </summary>
        public static NewOrderCreatedHandler NewOrderCreated = new NewOrderCreatedHandler { Name = "EcommerceEvents.NewOrderCreated" };

        /// <summary>
        /// Fired when existing order has been paid.
        /// </summary>
        public static OrderPaidHandler OrderPaid = new OrderPaidHandler { Name = "EcommerceEvents.OrderPaid" };

        /// <summary>
        /// Fired when a product was added to the shopping cart.
        /// </summary>
        public static ProductAddedToCartHandler ProductAddedToShoppingCart = new ProductAddedToCartHandler { Name = "EcommerceEvents.ProductAddedToCart" };
    }
}
