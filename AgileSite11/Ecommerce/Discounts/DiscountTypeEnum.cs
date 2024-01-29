using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Discount type.
    /// </summary>
    public enum DiscountTypeEnum
    {
        /// <summary>
        /// Product coupon
        /// </summary>
        [EnumStringRepresentation("ProductCoupon")]
        ProductCoupon = 0,


        /// <summary>
        /// Volume discount
        /// </summary>
        [EnumStringRepresentation("VolumeDiscount")]
        VolumeDiscount = 1,


        /// <summary>
        /// Custom discount
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("CustomDiscount")]
        CustomDiscount = 3,


        /// <summary>
        /// General discount 
        /// </summary>
        [EnumStringRepresentation("Discount")]
        Discount = 4,


        /// <summary>
        /// Catalog discount 
        /// </summary>
        [EnumStringRepresentation("CatalogDiscount")]
        CatalogDiscount = 5,


        /// <summary>
        /// Shipping discount 
        /// </summary>
        [EnumStringRepresentation("ShippingDiscount")]
        ShippingDiscount = 6,


        /// <summary>
        /// Order discount 
        /// </summary>
        [EnumStringRepresentation("OrderDiscount")]
        OrderDiscount = 7,


        /// <summary>
        /// Multibuy discount 
        /// </summary>
        [EnumStringRepresentation("MultibuyDiscount")]
        MultibuyDiscount = 8,


        /// <summary>
        /// GiftCard discount 
        /// </summary>
        [EnumStringRepresentation("GiftCard")]
        GiftCard = 9
    }
}