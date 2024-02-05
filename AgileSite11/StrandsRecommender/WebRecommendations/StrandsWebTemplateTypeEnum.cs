using CMS.Helpers;

namespace CMS.StrandsRecommender
{
    /// <summary>
    /// Enumeration describes type of recommendation widget.
    /// </summary>
    public enum StrandsWebTemplateTypeEnum
    {
        /// <summary>
        /// Home type, doesn't send any identifier
        /// </summary>
        [EnumStringRepresentation("Home")] // Represents name of the type how it appears on the Strands website. It is then used as a group caption at a dropdown list, so user can relate category shown in CMS and at Strands.
        Home,

        
        /// <summary>
        /// Category related type, sends category of currently visited page.
        /// </summary>
        [EnumStringRepresentation("Category")]
        Category,

        
        /// <summary>
        /// Product related type, sends identifier of currently visited product.
        /// </summary>
        [EnumStringRepresentation("Product Detail")]
        Product,

        
        /// <summary>
        /// Shopping cart related type, sends all items from shopping cart.
        /// </summary>
        [EnumStringRepresentation("Shopping Cart / Wishlist")]
        Cart,

        
        /// <summary>
        /// Order confirmation related type, send all recently purchased items.
        /// </summary>
        [EnumStringRepresentation("Order Confirmation")]
        Order,


        /// <summary>
        /// Miscellaneous type, not supported, recommendations with this type will be skipped.
        /// </summary>
        [EnumStringRepresentation("Miscellaneous")]
        Miscellaneous
    }
}