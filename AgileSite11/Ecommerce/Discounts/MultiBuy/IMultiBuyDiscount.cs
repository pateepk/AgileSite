using System;
using System.Collections.Generic;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface for discounts in form: "Buy N units of these products... and get M unit of these products for free".
    /// </summary>
    public interface IMultiBuyDiscount: IDiscount
    {
        /// <summary>
        /// Unique identifier of the discount.
        /// </summary>
        Guid DiscountGuid
        {
            get;
        }


        /// <summary>
        /// The number of products needed to enable this discount.
        /// </summary>
        int BasedOnUnitsCount
        {
            get;
        }


        /// <summary>
        /// The number of discounted units.
        /// </summary>
        int ApplyOnUnitsCount
        {
            get;
        }


        /// <summary>
        /// Indicates if further discounts are to be applied if this discount applies.
        /// </summary>
        bool ApplyFurtherDiscounts
        {
            get;
        }


        /// <summary>
        /// Indicates if product is added to cart automatically, the system adds product to shopping cart only when the discount is percentage and set 100 % off.
        /// </summary>
        bool AutoAddEnabled
        {
            get;
        }


        /// <summary>
        /// Maximum number of possible usages of the discount. Zero or negative value is representing an unlimited application count.
        /// </summary>
        int MaxApplication
        {
            get;
        }


        /// <summary>
        /// Sorts cart items according to priority. Places items preferred to be discounted on the beginning of list.
        /// </summary>
        /// <param name="items">Cart items to prioritize.</param>
        void PrioritizeItems(List<MultiBuyItem> items);


        /// <summary>
        /// Indicates if this discount is based on given cart item, i.e. this method returns true for items needed 
        /// to be in the cart to be eligible to get this discount.
        /// </summary>
        /// <param name="item">Item to check.</param>
        bool IsBasedOn(MultiBuyItem item);


        /// <summary>
        /// Indicates if this discount is affecting the price of given cart item, i.e. this method returns true for items 
        /// discounted by this discount.
        /// </summary>
        /// <param name="item">Item to check.</param>
        bool IsApplicableOn(MultiBuyItem item);


        /// <summary>
        /// Returns IDs of SKUs which could be discounted if present in cart. Most important products go first.
        /// </summary>
        IEnumerable<int> GetMissingProducts();
    }
}
