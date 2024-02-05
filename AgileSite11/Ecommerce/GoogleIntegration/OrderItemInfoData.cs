namespace CMS.Ecommerce
{
    /// <summary>
    /// Class encapsulates data related to the <see cref="OrderItemInfo"/>.
    /// </summary>
    internal class OrderItemInfoData
    {
        /// <summary>
        /// Contains data about order item.
        /// </summary>
        public OrderItemInfo OrderItem { get; set; }

        
        /// <summary>
        /// <see cref="SKUInfo"/> related to the <see cref="OrderItem"/>.
        /// </summary>
        public SKUInfo SKU { get; set; }


        /// <summary>
        /// <see cref="BrandInfo"/> related to the <see cref="SKU"/>, or null.
        /// </summary>
        public BrandInfo Brand { get; set; }
    }
}
