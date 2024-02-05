
namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents discount which is applicable only if specific macro condition is met.
    /// </summary>
    public interface IConditionalDiscount
    {
        /// <summary>
        /// Macro condition regarding products which must be satisfied to be able to use discount
        /// </summary>
        string DiscountProductCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Macro condition regarding shopping cart which must be satisfied to be able to use discount
        /// </summary>
        string DiscountCartCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates that further discounts (in order of DiscountOrder) are to be processed.
        /// </summary>
        bool ApplyFurtherDiscounts
        {
            get;
            set;
        }


        /// <summary>
        /// Discount order used to define its priority (1 is the highest priority).
        /// </summary>
        double DiscountItemOrder
        {
            get;
            set;
        }


        /// <summary>
        /// Minimum order discount amount.
        /// </summary>
        decimal DiscountItemMinOrderAmount
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if this discount is to be applied on the same base as the given discount.
        /// False indicates that discounts are to be applied one after another.
        /// </summary>
        /// <param name="discount">Examined discount.</param>
        bool ApplyTogetherWith(IConditionalDiscount discount);
    }
}
