using System;
using System.Collections.Generic;
using System.Linq;

using CMS.MacroEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Allows to filter list of conditional cart discounts according to its conditions and stop processing flag.
    /// </summary>
    public class CartDiscountsFilter : DiscountsFilterBase
    {
        /// <summary>
        /// Helper for correct macro resolver preparation.
        /// </summary>
        internal static class ResolverHelper
        {
            /// <summary>
            /// Sets source data in given resolver from given calculation data.
            /// </summary>
            internal static void PrepareResolver(MacroResolver resolver, CalculatorData data)
            {
                var request = data.Request;
                var result = data.Result;

                // Calculation data
                resolver.SetNamedSourceData("Data", data);
                resolver.SetNamedSourceData(nameof(request.BillingAddress), request.BillingAddress);
                resolver.SetNamedSourceData(nameof(request.ShippingAddress), request.ShippingAddress ?? request.BillingAddress);
                resolver.SetNamedSourceData(nameof(request.PaymentOption), request.PaymentOption);
                resolver.SetNamedSourceData(nameof(request.ShippingOption), request.ShippingOption);
                resolver.SetNamedSourceData(nameof(request.Currency), request.Currency);
                resolver.SetNamedSourceData(nameof(request.Customer), request.Customer);
                resolver.SetNamedSourceData(nameof(request.CalculationDate), request.CalculationDate);
                resolver.SetNamedSourceData(nameof(request.TotalItemsWeight), request.TotalItemsWeight);
                resolver.SetNamedSourceData(nameof(result.GrandTotal), result.GrandTotal);
                resolver.SetNamedSourceData(nameof(result.OrderDiscount), result.OrderDiscount);
                resolver.SetNamedSourceData(nameof(result.Shipping), result.Shipping);
                resolver.SetNamedSourceData(nameof(result.Subtotal), result.Subtotal);
                resolver.SetNamedSourceData(nameof(result.Tax), result.Tax);
                resolver.SetNamedSourceData(nameof(result.Total), result.Total);
                resolver.SetNamedSourceData(nameof(result.OrderDiscountSummary), result.OrderDiscountSummary);
                resolver.SetNamedSourceData(nameof(result.TaxSummary), result.TaxSummary);
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="calculation">Data from shopping cart calculation to be used for resolving cart discounts.</param>
        public CartDiscountsFilter(CalculatorData calculation)
        {
            if (calculation == null)
            {
                throw new ArgumentNullException(nameof(calculation));
            }

            if (calculation.Request.Currency == null)
            {
                throw new ArgumentNullException(nameof(calculation.Request.Currency));
            }

            if (calculation.Request.Site == null)
            {
                throw new ArgumentNullException(nameof(calculation.Request.Site));
            }

            ResolverHelper.PrepareResolver(Resolver, calculation);
        }


        /// <summary>
        /// Filters given discounts and leaves only those which conditions and min amounts are met.
        /// </summary>
        /// <param name="discounts">Conditional discounts to be filtered.</param>
        /// <param name="priceLimit">Only discounts with lower minimal amount than priceLimit will be left. Parameter is expected to be in the main currency and is ignored when set negative.</param>
        public virtual IEnumerable<IConditionalDiscount> Filter(IEnumerable<IConditionalDiscount> discounts, decimal priceLimit = -1)
        {
            // Filter discounts which can be applied on given price if limit is set
            if (priceLimit >= 0)
            {
                discounts = discounts.Where(x => x.DiscountItemMinOrderAmount <= priceLimit);
            }

            return FilterDiscounts(discounts);
        }


        /// <summary>
        /// Returns DiscountCartCondition from given discount object.
        /// </summary>
        /// <param name="discount">Discount to get cart condition from.</param>
        protected override string GetCondition(IConditionalDiscount discount)
        {
            return discount.DiscountCartCondition;
        }
    }
}

