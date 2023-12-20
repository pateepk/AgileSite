namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a general discount application which is applicable on the base price.
    /// </summary>
    /// <remarks>
    /// Use this class as a base for custom discount calculation implementations.
    /// </remarks>
    public interface IDiscount : ICouponCodeApplication
    {
        /// <summary>
        /// Gets the display name of the discount.
        /// </summary>
        string DiscountName
        {
            get;
        }


        /// <summary>
        /// Applied coupon code.
        /// </summary>
        string AppliedCouponCode
        {
            get;
            set;
        }
        

        /// <summary>
        /// Calculates the discount value for given <paramref name="basePrice"/>.
        /// </summary>
        /// <remarks>
        /// It is expected to return negative discount value for negative <paramref name="basePrice"/>.
        /// </remarks>
        /// <param name="basePrice">The price to calculate discount from.</param>
        /// <returns>Rounded discount value.</returns>
        decimal CalculateDiscount(decimal basePrice);
    }
}