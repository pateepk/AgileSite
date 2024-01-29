using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Core;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides cart item discount calculation (product coupon and multibuy discounts).
    /// </summary>
    public class CartItemDiscountCalculator : IShoppingCartCalculator
    {
        /// <summary>
        /// Calculates the values of multibuy discounts and stores them to <see cref="CalculationResultItem.ItemDiscount"/> property.
        /// </summary>
        /// <param name="calculationData">All calculation related data.</param>
        public void Calculate(CalculatorData calculationData)
        {
            var request = calculationData.Request;
            var result = calculationData.Result;
            var resultItems = result.Items.ToList();
            var parameters = PrepareParameters(request);

            // Convert calculation items to multibuy items
            var items = request.Items.Join(resultItems,
                                req => req.ItemGuid,
                                res => res.ItemGuid,
                                (req, res) => new MultiBuyItem(req, res.ItemUnitPrice))
                                .ToList();

            // Evaluate product coupon discounts
            Service.Resolve<IProductCouponService>().EvaluateDiscounts(items, parameters, new CouponApplicator(result));

            // Evaluate multibuy discounts
            Service.Resolve<IMultiBuyDiscountsService>().EvaluateDiscounts(items, parameters, new MultiBuyApplicator(result));
        }


        private static DiscountsParameters PrepareParameters(CalculationRequest request)
        {
            var param = new DiscountsParameters
            {
                SiteID = request.Site,
                DueDate = request.CalculationDate,
                Enabled = true,
                User = request.User,
                Currency = request.Currency,
                CouponCodes = request.CouponCodes
            };

            return param;
        }
        

        #region "Product Coupon Applicator Class"

        internal class CouponApplicator : IMultiBuyDiscountsApplicator
        {
            private readonly Dictionary<Guid, CalculationResultItem> mItemsByGuid;
            private readonly HashSet<Guid> appliedDiscounts = new HashSet<Guid>();
            private readonly CalculationResult mResult;


            public CouponApplicator(CalculationResult result)
            {
                mItemsByGuid = result.Items.ToDictionary(item => item.ItemGuid);
                mResult = result;
            }


            public void ApplyDiscount(IMultiBuyDiscount discount, MultiBuyItem itemToBeDiscounted, int? units = null)
            {
                CalculationResultItem item;
                if (mItemsByGuid.TryGetValue(itemToBeDiscounted.ID, out item))
                {
                    var discountValue = discount.CalculateDiscount(itemToBeDiscounted.UnitPrice);
                    var totalDiscount = (units ?? itemToBeDiscounted.Units) * discountValue;

                    item.ItemDiscount += totalDiscount;
                    item.ItemDiscountSummary.Sum(discount.DiscountName, totalDiscount);

                    // Modify item unit price for further calculation
                    itemToBeDiscounted.UnitPrice -= discountValue;
                }

                var newDiscountApplied = appliedDiscounts.Add(discount.DiscountGuid);
                if (newDiscountApplied && !string.IsNullOrEmpty(discount.AppliedCouponCode))
                {
                    // Log coupon code usage for coupon code triggered discount
                    mResult.AppliedCouponCodes.Add(new CouponCode(discount.AppliedCouponCode, CouponCodeApplicationStatusEnum.AppliedInCart, discount));
                }
            }


            public void Reset()
            {
                // Not supported in discount applicators
            }


            public bool AcceptsMissedDiscount(IMultiBuyDiscount discount, int missedApplications)
            {
                return false;
            }
        }

        #endregion


        #region "Multibuy Discounts Applicator Class"

        internal class MultiBuyApplicator : IMultiBuyDiscountsApplicator
        {
            private readonly Dictionary<Guid, CalculationResultItem> mItemsByGuid;
            private readonly HashSet<Guid> appliedDiscounts = new HashSet<Guid>();
            private readonly CalculationResult mResult;


            public MultiBuyApplicator(CalculationResult result)
            {
                mItemsByGuid = result.Items.ToDictionary(item => item.ItemGuid);
                mResult = result;
            }


            public void ApplyDiscount(IMultiBuyDiscount discount, MultiBuyItem itemToBeDiscounted, int? units = null)
            {
                CalculationResultItem item;
                if (mItemsByGuid.TryGetValue(itemToBeDiscounted.ID, out item))
                {
                    var value = (units ?? itemToBeDiscounted.Units) * discount.CalculateDiscount(itemToBeDiscounted.UnitPrice);

                    item.ItemDiscount += value;
                    item.ItemDiscountSummary.Sum(discount.DiscountName, value);
                }

                var newDiscountApplied = appliedDiscounts.Add(discount.DiscountGuid);
                if (newDiscountApplied && !string.IsNullOrEmpty(discount.AppliedCouponCode))
                {
                    // Log coupon code usage for coupon code triggered discount
                    mResult.AppliedCouponCodes.Add(new CouponCode(discount.AppliedCouponCode, CouponCodeApplicationStatusEnum.AppliedInCart, discount));
                }
            }


            public void Reset()
            {
                // Not supported in discount applicators
            }


            public bool AcceptsMissedDiscount(IMultiBuyDiscount discount, int missedApplications = 1)
            {
                return false;
            }
        }

        #endregion
    }
}
