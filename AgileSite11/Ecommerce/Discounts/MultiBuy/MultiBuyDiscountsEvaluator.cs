using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class handling application of multi buy discounts on set of cart items. Discounts are based on the most expensive products. 
    /// Discounts are applied to cheapest products.
    /// </summary>
    public class MultiBuyDiscountsEvaluator : IMultiBuyDiscountsEvaluator
    {
        #region "Variables"

        private List<MultiBuyItem> mSortedItems;

        #endregion


        #region "Properties"

        /// <summary>
        /// Original list of cart items currently processed by this applicator.
        /// </summary>
        private List<MultiBuyItem> OriginalItems
        {
            get;
            set;
        } = new List<MultiBuyItem>();


        /// <summary>
        /// List of cart items currently processed by this applicator.
        /// </summary>
        protected List<MultiBuyItem> PrioritizedItems
        {
            get;
            set;
        }


        /// <summary>
        /// Cart items sorted by price.
        /// </summary>
        protected IEnumerable<MultiBuyItem> SortedItems
        {
            get
            {
                if (mSortedItems == null)
                {
                    mSortedItems = new List<MultiBuyItem>(OriginalItems);

                    // Sort items from cheapest to most expensive
                    mSortedItems.Sort(CompareByUnitPriceWithOptions);
                }

                return mSortedItems;
            }
        }


        /// <summary>
        /// Dictionary containing the number of uses for each cart item.
        /// </summary>
        private Dictionary<MultiBuyItem, int> UsedDiscountItems
        {
            get;
        } = new Dictionary<MultiBuyItem, int>();


        /// <summary>
        /// MultiBuy discounts applicator to be used for application of items.
        /// </summary>
        private IMultiBuyDiscountsApplicator Applicator
        {
            get;
            set;
        }

        #endregion


        #region "Discounts evaluation"

        /// <summary>
        /// Resets evaluator to its initial state.
        /// </summary>
        protected virtual void Reset()
        {
            // Reset applicator
            Applicator?.Reset();

            OriginalItems.ForEach(item => UsedDiscountItems[item] = 0);
            mSortedItems = null;
        }


        /// <summary>
        /// Evaluates given discounts and applies matching ones to corresponding cart items.
        /// </summary>
        /// <param name="discounts">Discounts to be evaluated.</param>
        /// <param name="cartItems">Cart items to be evaluated.</param>
        /// <param name="applicator">MultiBuy discounts applicator to be used for application of items.</param>
        public void EvaluateDiscounts(IEnumerable<IMultiBuyDiscount> discounts, IEnumerable<MultiBuyItem> cartItems, IMultiBuyDiscountsApplicator applicator)
        {
            OriginalItems = cartItems.ToList();
            Applicator = applicator;
            Reset();

            foreach (var discount in discounts)
            {
                // Prepare items list for discount application
                PrepareItemsForDiscount(discount);

                var applied = TryApplyDiscount(discount);

                // Stop processing when applied and discount does not allow processing further discounts
                if (applied && !discount.ApplyFurtherDiscounts)
                {
                    break;
                }
            }
        }


        /// <summary>
        /// Ensures that PrioritizedItems list is ready for application of given discount. 
        /// </summary>
        /// <param name="discount">Discount to prepare items for.</param>
        protected virtual void PrepareItemsForDiscount(IMultiBuyDiscount discount)
        {
            // Use sorted list of items
            PrioritizedItems = new List<MultiBuyItem>(SortedItems);

            // Let discount prioritize items
            discount.PrioritizeItems(PrioritizedItems);
        }


        /// <summary>
        /// Evaluates given discount against all cart items and applies usable of them. Returns true if discount was applied at least once.
        /// </summary>
        /// <param name="discount">Discount to be evaluated.</param>
        private bool TryApplyDiscount(IMultiBuyDiscount discount)
        {
            var itemsToBeDiscounted = new List<MultiBuyItem>();
            var itemsToBaseDiscountOn = new List<MultiBuyItem>();

            // Find discount application opportunity
            var isApplicable = FindDiscountApplication(discount, itemsToBaseDiscountOn, itemsToBeDiscounted);

            var applicationCount = 0;

            if (isApplicable)
            {
                applicationCount = ApplyDiscount(discount, itemsToBaseDiscountOn, itemsToBeDiscounted);
            }

            var missedDiscountsFound = FindMissedDiscounts(discount, itemsToBaseDiscountOn, applicationCount);

            // Return true if discount was applied at least once
            return missedDiscountsFound || (applicationCount > 0);
        }


        /// <summary>
        /// Evaluates the unused units and searches for the missed discounts.
        /// Returns <c>true</c> if the missed applications was accepted.
        /// </summary>
        /// <param name="discount">Evaluated discount</param>
        /// <param name="itemsToBaseDiscountOn">Collection of the discounting items.</param>
        /// <param name="usedApplicationCount">Indicates how many times was the discount already applied.</param>
        private bool FindMissedDiscounts(IMultiBuyDiscount discount, ICollection<MultiBuyItem> itemsToBaseDiscountOn, int usedApplicationCount)
        {
            // Find some unused buyX applications
            var unusedXs = itemsToBaseDiscountOn.Select(i => new KeyValuePair<MultiBuyItem, int>(i, GetUnusedNonFreeUnits(i, false))).ToList();

            var unusedBuyXUnits = unusedXs.Sum(i => i.Value);
            var missedBuyXApplication = unusedBuyXUnits / discount.BasedOnUnitsCount;

            if (discount.MaxApplication > 0)
            {
                // Check if there is enough discount applications
                missedBuyXApplication = Math.Min(missedBuyXApplication, discount.MaxApplication - usedApplicationCount);
            }

            if ((missedBuyXApplication <= 0) || (Applicator == null))
            {
                // Nothing to auto-add
                return false;
            }

            if (Applicator.AcceptsMissedDiscount(discount, missedBuyXApplication))
            {
                var missedBuyXTotalUnits = missedBuyXApplication * discount.BasedOnUnitsCount;
                UpdateItemsUsage(GetUsedSubset(unusedXs, missedBuyXTotalUnits));

                return true;
            }

            return false;
        }


        #region "Searching for items needed for discount to be applied"

        /// <summary>
        /// Finds opportunity to apply given discount. This method adds potential discounting items with unit counts into itemsToBaseDiscountOn collection. 
        /// Potentially discounted items with discounted units counts are added to itemsToBeDiscounted collection.
        /// </summary>
        /// <param name="discount">Discount to find opportunity for.</param>
        /// <param name="itemsToBaseDiscountOn">Empty collection of items to base discount on. 
        /// This method adds found required items here.</param>
        /// <param name="itemsToBeDiscounted">Empty collection of items to be discounted.
        /// This method adds found items discounted by given discount here.</param>
        /// <returns>Returns true when given discount is applicable.</returns>
        private bool FindDiscountApplication(IMultiBuyDiscount discount, ICollection<MultiBuyItem> itemsToBaseDiscountOn, ICollection<MultiBuyItem> itemsToBeDiscounted)
        {
            if (!FindItemsToBaseDiscountOn(discount, itemsToBaseDiscountOn))
            {
                return false;
            }

            return FindItemsToBeDiscounted(discount, itemsToBeDiscounted);
        }


        /// <summary>
        /// Finds items satisfying given discounts conditions.
        /// </summary>
        /// <param name="discount">Discount to find items for.</param>
        /// <param name="itemsToBaseDiscountOn">Empty collection of items. Found items are placed here.</param>
        /// <returns>Returns true if found items fully satisfies discount.</returns>
        protected virtual bool FindItemsToBaseDiscountOn(IMultiBuyDiscount discount, ICollection<MultiBuyItem> itemsToBaseDiscountOn)
        {
            var baseOnCount = 0;

            // Find item with lowest priority which satisfies discount conditions
            var discounting = PrioritizedItems.Where(item => CanBaseDiscountOn(item, discount, itemsToBaseDiscountOn)).Reverse();

            foreach (var item in discounting)
            {
                var availableCount = GetUnusedNonFreeUnits(item, false);
                itemsToBaseDiscountOn.Add(item);

                baseOnCount += availableCount;
            }

            return baseOnCount >= discount.BasedOnUnitsCount;
        }


        /// <summary>
        /// Finds items to be discounted with given discount based on given items. Returns true when at least one item was found.
        /// </summary>
        /// <param name="discount">Discount to find suitable items for.</param>
        /// <param name="itemsToBeDiscounted">Empty collection of items. Found items are placed here.</param>
        protected virtual bool FindItemsToBeDiscounted(IMultiBuyDiscount discount, ICollection<MultiBuyItem> itemsToBeDiscounted)
        {
            var getYCount = 0;

            // Find item with highest priority which can be discounted
            var discounted = PrioritizedItems.Where(item => CanBeDiscounted(item, discount));

            foreach (var item in discounted)
            {
                var availableCount = GetUnusedNonFreeUnits(item, discount.AutoAddEnabled);
                itemsToBeDiscounted.Add(item);

                getYCount += availableCount;
            }

            return getYCount >= discount.ApplyOnUnitsCount;
        }


        /// <summary>
        /// Checks if given item can be discounted using given discount.
        /// </summary>
        /// <param name="item">Item to be checked.</param>
        /// <param name="discount">Discount to be checked.</param>
        /// <returns>Returns true if discount is applicable on given item.</returns>
        protected virtual bool CanBeDiscounted(MultiBuyItem item, IMultiBuyDiscount discount)
        {
            // Only paid items having some unit not used as base can be used
            var availableUnits = GetUnusedNonFreeUnits(item, discount.AutoAddEnabled);
            var hasEnoughUnits = (availableUnits > 0);

            return hasEnoughUnits && discount.IsApplicableOn(item);
        }


        /// <summary>
        /// Checks if given item can be used to fulfill discounts conditions.
        /// </summary>
        /// <param name="item">Item to be checked.</param>
        /// <param name="discount">Discount to be checked.</param>
        /// <param name="itemsToBaseDiscountOn">Units with number of units already found as base for given discount.</param>
        /// <returns>Returns true if item covers (partially) discounts condition.</returns>
        protected virtual bool CanBaseDiscountOn(MultiBuyItem item, IMultiBuyDiscount discount, ICollection<MultiBuyItem> itemsToBaseDiscountOn)
        {
            var availableUnits = GetUnusedNonFreeUnits(item, false);

            var hasEnoughUnits = (availableUnits > 0);

            return hasEnoughUnits && discount.IsBasedOn(item);
        }

        #endregion


        #region "Discount application"

        /// <summary>
        /// Applies the given <paramref name="discount"/> and returns the application count.
        /// Discount is applied and corresponding items are marked as used.
        /// </summary>
        /// <param name="discount">Discount to apply.</param>
        /// <param name="itemsToBaseDiscountOn">Discounting items.</param>
        /// <param name="itemsToBeDiscounted">Discounted items.</param>
        private int ApplyDiscount(IMultiBuyDiscount discount, ICollection<MultiBuyItem> itemsToBaseDiscountOn, ICollection<MultiBuyItem> itemsToBeDiscounted)
        {
            var buyXs = itemsToBaseDiscountOn.Select(i => new KeyValuePair<MultiBuyItem, int>(i, GetUnusedNonFreeUnits(i, false))).ToList();
            var getYs = itemsToBeDiscounted.Select(i => new KeyValuePair<MultiBuyItem, int>(i, GetUnusedNonFreeUnits(i, discount.AutoAddEnabled))).ToList();

            var applicationCount = GetApplicationCount(discount, buyXs, getYs);

            // Calculate total units of X and Y sets
            var buyXTotal = applicationCount * discount.BasedOnUnitsCount;
            var getYTotal = applicationCount * discount.ApplyOnUnitsCount;

            var usedXs = GetUsedSubset(buyXs, buyXTotal).ToList();
            var usedYs = GetUsedSubset(getYs, getYTotal).ToList();

            // Remember usage for all used Xs and Ys
            UpdateItemsUsage(usedXs);
            UpdateItemsUsage(usedYs);

            // Apply discount on Ys
            usedYs.ForEach(i => ApplyDiscount(discount, i.Key, i.Value));

            return applicationCount;
        }


        /// <summary>
        /// Calculates the number of possible <paramref name="discount"/> applications.
        /// </summary>
        /// <param name="discount">Evaluated discount.</param>
        /// <param name="itemsToBaseDiscountOn">Set of discounting items and their unit count.</param>
        /// <param name="itemsToBeDiscounted">Set of discounted items and their unit count.</param>
        private int GetApplicationCount(IMultiBuyDiscount discount, ICollection<KeyValuePair<MultiBuyItem, int>> itemsToBaseDiscountOn, ICollection<KeyValuePair<MultiBuyItem, int>> itemsToBeDiscounted)
        {
            var totalUnitsCount = itemsToBaseDiscountOn.ToList().Union(itemsToBeDiscounted)
                                                       .GroupBy(x => x.Key)
                                                       .Sum(g => g.Max(i => i.Value));

            var buyXCount = discount.BasedOnUnitsCount;
            var getYCount = discount.ApplyOnUnitsCount;

            var applications = totalUnitsCount / (buyXCount + getYCount);

            if (discount.MaxApplication > 0)
            {
                // Limit max applications
                applications = applications > discount.MaxApplication ? discount.MaxApplication : applications;
            }

            // Check if group count is achievable in buyX and getY sets
            var buyXgroupCount = itemsToBaseDiscountOn.Sum(i => i.Value) / buyXCount;
            var getYgroupCount = itemsToBeDiscounted.Sum(i => i.Value) / getYCount;

            return new[]
            {
                applications,
                buyXgroupCount,
                getYgroupCount
            }.Min();
        }


        /// <summary>
        /// Applies discount to given number of unit of given item using.
        /// </summary>
        /// <param name="discount">Discount to be applied.</param>
        /// <param name="itemToBeDiscounted">Cart item to apply discount on.</param>
        /// <param name="units">Number of unit to be discounted.</param>
        protected virtual void ApplyDiscount(IMultiBuyDiscount discount, MultiBuyItem itemToBeDiscounted, int units)
        {
            Applicator?.ApplyDiscount(discount, itemToBeDiscounted, units);
        }

        #endregion


        #region "Management of items already used as base for discount."

        /// <summary>
        /// Returns the number of unit not used as base for any discount nor discounted.
        /// </summary>
        /// <param name="item">Item to get number of units for.</param>
        /// <param name="includingAutoAddedUnits">Indicates if the result contains also automatically added units.</param>
        protected virtual int GetUnusedNonFreeUnits(MultiBuyItem item, bool includingAutoAddedUnits)
        {
            var itemUnits = item.Units;

            if (!includingAutoAddedUnits)
            {
                itemUnits -= item.AutoAddedUnits;
            }

            return (int)itemUnits - UsedDiscountItems[item];
        }


        /// <summary>
        /// Updates the <paramref name="items"/> usage to the <see cref="UsedDiscountItems"/> collection.
        /// </summary>
        /// <param name="items">Items to be marked as used</param>
        private void UpdateItemsUsage(IEnumerable<KeyValuePair<MultiBuyItem, int>> items)
        {
            items.ToList().ForEach(i => UsedDiscountItems[i.Key] += i.Value);
        }


        /// <summary>
        /// Returns the subset of the <paramref name="items"/>.
        /// Items are returned in the order specified in the <paramref name="items"/> collection.
        /// Subset total unit count is limited by <paramref name="maxUsedItems"/> units count.
        /// </summary>
        /// <param name="items">Collection of the items and their unit count.</param>
        /// <param name="maxUsedItems">Maximal unit</param>
        private IEnumerable<KeyValuePair<MultiBuyItem, int>> GetUsedSubset(IEnumerable<KeyValuePair<MultiBuyItem, int>> items, int maxUsedItems)
        {
            foreach (var item in items)
            {
                if (maxUsedItems <= 0)
                {
                    break;
                }

                var count = item.Value;
                var discountedCount = (count > maxUsedItems) ? maxUsedItems : count;
                maxUsedItems -= discountedCount;

                yield return new KeyValuePair<MultiBuyItem, int>(item.Key, discountedCount);
            }
        }

        #endregion

        #endregion


        #region "Helper methods"

        private int CompareByUnitPriceWithOptions(MultiBuyItem item1, MultiBuyItem item2)
        {
            var price1 = item1.UnitPrice;
            var price2 = item2.UnitPrice;

            return price1.CompareTo(price2);
        }

        #endregion
    }
}
