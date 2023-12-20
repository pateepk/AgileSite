namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface implemented by applicators of multibuy/coupon discounts and auto-adders.
    /// </summary>
    public interface IMultiBuyDiscountsApplicator
    {
        /// <summary>
        /// Resets applicator to its initial state.
        /// </summary>
        void Reset();


        /// <summary>
        /// Applies discount to given number of unit of given item.
        /// </summary>
        /// <param name="discount">Discount to be applied.</param>
        /// <param name="itemToBeDiscounted">Cart item to apply discount on.</param>
        /// <param name="units">Number of unit to be discounted. Can be used to override value from <paramref name="itemToBeDiscounted"/> (<see cref="MultiBuyItem.Units"/>).</param>
        void ApplyDiscount(IMultiBuyDiscount discount, MultiBuyItem itemToBeDiscounted, int? units = null);


        /// <summary>
        /// Notifies the applicator that discount was nearly applied. 
        /// It could be applied on some products if present in cart.
        /// </summary>
        /// <param name="discount">Nearly applied discount.</param>
        /// <param name="missedApplications">Missed application count.</param>
        bool AcceptsMissedDiscount(IMultiBuyDiscount discount, int missedApplications);
    }
}
